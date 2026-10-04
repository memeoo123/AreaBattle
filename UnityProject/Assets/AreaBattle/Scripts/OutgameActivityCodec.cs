using System;
using System.IO;
using System.IO.Compression;
using System.Text;
namespace AreaBattle
{
    // ActivityUtils4647 has its own stream disposal scopes, unlike the statistics helper.
    public static class OutgameActivityCodec
    {
        public static string CompressString(string text)
        {if(string.IsNullOrEmpty(text))return "";return Convert.ToBase64String(Compress(Encoding.UTF8.GetBytes(text.ToString())));}
        public static byte[] Compress(byte[] data)
        {
            using(var output=new MemoryStream())
            {
                using(var gzip=new GZipStream(output,CompressionMode.Compress,true))
                {gzip.Write(data,0,data.Length);gzip.Close();}
                return output.ToArray();
            }
        }
        public static string DecompressString(string text,Action<string> warning)
        {
            if(string.IsNullOrEmpty(text))return "";
            try{return Encoding.UTF8.GetString(Decompress(Convert.FromBase64String(text.ToString())));}
            catch(Exception ex){warning("解压缩出错: "+ex.Message);return text;}
        }
        public static byte[] Decompress(byte[] data)
        {
            using(var input=new MemoryStream(data))
            using(var gzip=new GZipStream(input,CompressionMode.Decompress))
            using(var output=new MemoryStream())
            {
                var buffer=new byte[1024];int count;
                while((count=gzip.Read(buffer,0,buffer.Length))>=1)output.Write(buffer,0,count);
                return output.ToArray();
            }
        }
    }
}
