using System;
using System.IO;
using System.IO.Compression;
using System.Text;
namespace AreaBattle
{
    // StatistUtils4619: UTF8 -> gzip -> base64; failed string decompression returns its original input.
    public static class OutgameStatisticsCodec
    {
        public static string CompressString(string text)
        {if(string.IsNullOrEmpty(text))return "";return Convert.ToBase64String(Compress(Encoding.UTF8.GetBytes(text.ToString())));}
        public static byte[] Compress(byte[] data)
        {
            var output=new MemoryStream();var gzip=new GZipStream(output,CompressionMode.Compress,true);
            gzip.Write(data,0,data.Length);gzip.Close();return output.ToArray();
        }
        public static string DecompressString(string text,Action<string> warning)
        {
            if(string.IsNullOrEmpty(text))return "";
            try{return Encoding.UTF8.GetString(Decompress(Convert.FromBase64String(text.ToString())));}
            catch(Exception ex){warning("解压缩出错: "+ex.Message);return text;}
        }
        public static byte[] Decompress(byte[] data)
        {
            var input=new MemoryStream(data);var gzip=new GZipStream(input,CompressionMode.Decompress);
            var output=new MemoryStream();var buffer=new byte[1024];int count;
            while((count=gzip.Read(buffer,0,buffer.Length))>=1)output.Write(buffer,0,count);
            gzip.Close();return output.ToArray();
        }
    }
}
