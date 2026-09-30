#!/usr/bin/env node
'use strict';

// Windows wxapkg decryption was cross-checked against unbyte/unwx 0.4.0,
// commit 3a03465fb84d66e0a8a81e6f58b6c3636934d961 (MIT).
// Archive validation and path containment are implemented independently here.

const crypto = require('node:crypto');
const fs = require('node:fs');
const path = require('node:path');

function fail(message) {
  throw new Error(message);
}

function parseArgs(argv) {
  const values = {};
  for (let i = 0; i < argv.length; i += 2) {
    const key = argv[i];
    const value = argv[i + 1];
    if (!key?.startsWith('--') || value === undefined) {
      fail('Usage: node unpack_wxapkg.js --input FILE --output DIR --wxid wx... --report FILE');
    }
    values[key.slice(2)] = value;
  }
  for (const required of ['input', 'output', 'wxid', 'report']) {
    if (!values[required]) fail(`Missing --${required}`);
  }
  if (!/^wx[0-9a-z]+$/i.test(values.wxid)) fail('Invalid wxid');
  return values;
}

function sha256(buffer) {
  return crypto.createHash('sha256').update(buffer).digest('hex');
}

function decryptWindowsPackage(data, wxid) {
  if (data.length < 1030 || !data.subarray(0, 6).equals(Buffer.from('V1MMWX'))) {
    return { decrypted: data, encrypted: false };
  }

  const key = crypto.pbkdf2Sync(wxid, 'saltiest', 1000, 32, 'sha1');
  const decipher = crypto.createDecipheriv('aes-256-cbc', key, Buffer.from('the iv: 16 bytes'));
  decipher.setAutoPadding(false);
  const headerBlock = Buffer.concat([
    decipher.update(data.subarray(6, 1030)),
    decipher.final(),
  ]);
  const decryptedHeader = headerBlock.subarray(0, headerBlock.length - 1);

  const wxidBytes = Buffer.from(wxid, 'utf8');
  const xorKey = wxidBytes.length >= 2 ? wxidBytes[wxidBytes.length - 2] : 0x66;
  const tail = Buffer.allocUnsafe(data.length - 1030);
  for (let i = 1030; i < data.length; i += 1) {
    tail[i - 1030] = data[i] ^ xorKey;
  }
  return { decrypted: Buffer.concat([decryptedHeader, tail]), encrypted: true };
}

function readU32BE(buffer, cursor, label) {
  if (cursor.offset + 4 > buffer.length) fail(`Truncated ${label}`);
  const value = buffer.readUInt32BE(cursor.offset);
  cursor.offset += 4;
  return value;
}

function safeOutputPath(outputRoot, archiveName) {
  if (archiveName.includes('\0')) fail('NUL byte in archive path');
  const normalized = archiveName.replaceAll('\\', '/').replace(/^\/+/, '');
  const parts = normalized.split('/');
  if (!normalized || parts.some((part) => !part || part === '.' || part === '..' || part.includes(':'))) {
    fail(`Unsafe archive path: ${JSON.stringify(archiveName)}`);
  }
  const destination = path.resolve(outputRoot, ...parts);
  const prefix = `${path.resolve(outputRoot)}${path.sep}`;
  if (!destination.startsWith(prefix)) fail(`Archive path escapes output root: ${archiveName}`);
  return { normalized, destination };
}

function decodeEntries(container, outputRoot) {
  if (container.length < 18 || container[0] !== 0xbe || container[13] !== 0xed) {
    fail('Invalid decrypted wxapkg header markers');
  }
  const fileInfoOffset = container.readUInt32BE(1);
  const indexInfoLength = container.readUInt32BE(5);
  const bodyInfoLength = container.readUInt32BE(9);
  const fileCount = container.readUInt32BE(14);
  if (fileCount > 100000) fail(`Unreasonable file count: ${fileCount}`);

  const cursor = { offset: 18 };
  const decoder = new TextDecoder('utf-8', { fatal: true });
  const seen = new Set();
  const entries = [];
  for (let i = 0; i < fileCount; i += 1) {
    const nameLength = readU32BE(container, cursor, `name length for entry ${i}`);
    if (nameLength === 0 || cursor.offset + nameLength > container.length) {
      fail(`Invalid name length for entry ${i}: ${nameLength}`);
    }
    const name = decoder.decode(container.subarray(cursor.offset, cursor.offset + nameLength));
    cursor.offset += nameLength;
    const offset = readU32BE(container, cursor, `offset for ${name}`);
    const size = readU32BE(container, cursor, `size for ${name}`);
    if (offset + size > container.length || offset + size < offset) {
      fail(`Out-of-bounds entry: ${name}`);
    }
    const safe = safeOutputPath(outputRoot, name);
    const key = safe.normalized.toLowerCase();
    if (seen.has(key)) fail(`Duplicate archive path: ${safe.normalized}`);
    seen.add(key);
    entries.push({
      archivePath: safe.normalized,
      destination: safe.destination,
      offset,
      size,
      data: container.subarray(offset, offset + size),
    });
  }

  return { fileInfoOffset, indexInfoLength, bodyInfoLength, fileCount, entries };
}

function ensureEmptyOutput(outputRoot) {
  if (fs.existsSync(outputRoot) && fs.readdirSync(outputRoot).length !== 0) {
    fail(`Refusing to write into non-empty output directory: ${outputRoot}`);
  }
  fs.mkdirSync(outputRoot, { recursive: true });
}

function main() {
  const args = parseArgs(process.argv.slice(2));
  const input = path.resolve(args.input);
  const outputRoot = path.resolve(args.output);
  const reportPath = path.resolve(args.report);
  const source = fs.readFileSync(input);
  const sourceHash = sha256(source);
  const { decrypted, encrypted } = decryptWindowsPackage(source, args.wxid);
  const decoded = decodeEntries(decrypted, outputRoot);

  ensureEmptyOutput(outputRoot);
  let totalExtractedBytes = 0;
  const reportEntries = [];
  for (const entry of decoded.entries) {
    fs.mkdirSync(path.dirname(entry.destination), { recursive: true });
    fs.writeFileSync(entry.destination, entry.data, { flag: 'wx' });
    totalExtractedBytes += entry.size;
    reportEntries.push({
      path: entry.archivePath,
      offset: entry.offset,
      size: entry.size,
      sha256: sha256(entry.data),
    });
  }

  const report = {
    schemaVersion: '1.0',
    tool: {
      name: 'workspace-safe-wxapkg-unpacker',
      runtime: process.version,
      algorithmReference: {
        project: 'unbyte/unwx',
        version: '0.4.0',
        commit: '3a03465fb84d66e0a8a81e6f58b6c3636934d961',
        license: 'MIT',
      },
    },
    source: {
      path: input,
      size: source.length,
      sha256: sourceHash,
      encryptedWindowsEnvelope: encrypted,
    },
    decryptedContainer: {
      size: decrypted.length,
      sha256: sha256(decrypted),
      fileInfoOffset: decoded.fileInfoOffset,
      indexInfoLength: decoded.indexInfoLength,
      bodyInfoLength: decoded.bodyInfoLength,
    },
    outputRoot,
    fileCount: decoded.fileCount,
    totalExtractedBytes,
    entries: reportEntries,
  };
  fs.mkdirSync(path.dirname(reportPath), { recursive: true });
  fs.writeFileSync(reportPath, `${JSON.stringify(report, null, 2)}\n`, { flag: 'wx' });
  process.stdout.write(`${JSON.stringify({
    outputRoot,
    fileCount: decoded.fileCount,
    totalExtractedBytes,
    sourceSha256: sourceHash,
    decryptedSha256: report.decryptedContainer.sha256,
    report: reportPath,
  }, null, 2)}\n`);
}

try {
  main();
} catch (error) {
  process.stderr.write(`${error.stack || error.message}\n`);
  process.exitCode = 1;
}
