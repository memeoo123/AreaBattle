using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
namespace AreaBattle
{
    // GFRunning.Json3133 / nested parser3132, Util.DataParse25122.
    public static class OutgameCallbackJson
    {
        public static Dictionary<string,object> DataParse(string text,Action<string> log)
        {
            log("get callback data: "+text);
            return Deserialize(text) as Dictionary<string,object>;
        }
        public static object Deserialize(string text)
        {
            if(text==null)return null;
            using(var parser=new Parser(text))return parser.Value();
        }
        sealed class Parser:IDisposable
        {
            enum Token{None,ObjectOpen,ObjectClose,ArrayOpen,ArrayClose,Colon,Comma,String,Number,True,False,Null}
            readonly StringReader reader;
            public Parser(string text){reader=new StringReader(text);}
            public void Dispose()=>reader.Dispose();
            char Peek=>Convert.ToChar(reader.Peek());
            char Next=>Convert.ToChar(reader.Read());
            static bool WordBreak(char c)=>char.IsWhiteSpace(c)||"{}[],:\"".IndexOf(c)!=-1;
            string Word()
            {
                var s=new StringBuilder();
                while(reader.Peek()!=-1&&!WordBreak(Peek))s.Append(Next);
                return s.ToString();
            }
            Token NextToken()
            {
                while(reader.Peek()!=-1&&char.IsWhiteSpace(Peek))reader.Read();
                if(reader.Peek()==-1)return Token.None;
                char c=Peek;
                switch(c)
                {
                    case '{':return Token.ObjectOpen;case '}':reader.Read();return Token.ObjectClose;
                    case '[':return Token.ArrayOpen;case ']':reader.Read();return Token.ArrayClose;
                    case ',':reader.Read();return Token.Comma;case ':':return Token.Colon;
                    case '"':return Token.String;
                }
                if(c=='-'||(c>='0'&&c<='9'))return Token.Number;
                switch(Word()){case "true":return Token.True;case "false":return Token.False;case "null":return Token.Null;default:return Token.None;}
            }
            public object Value()=>ByToken(NextToken());
            object ByToken(Token token)
            {
                switch(token)
                {
                    case Token.String:return String();case Token.Number:return Number();
                    case Token.ObjectOpen:return Object();case Token.ArrayOpen:return Array();
                    case Token.True:return true;case Token.False:return false;default:return null;
                }
            }
            Dictionary<string,object> Object()
            {
                var result=new Dictionary<string,object>();reader.Read();
                while(true)
                {
                    switch(NextToken())
                    {
                        case Token.None:return null;case Token.Comma:continue;case Token.ObjectClose:return result;
                    }
                    string name=String();if(name==null)return null;
                    if(NextToken()!=Token.Colon)return null;
                    reader.Read();result[name]=Value();
                }
            }
            List<object> Array()
            {
                var result=new List<object>();reader.Read();
                while(true)
                {
                    Token token=NextToken();
                    switch(token){case Token.None:return null;case Token.Comma:continue;case Token.ArrayClose:return result;}
                    result.Add(ByToken(token));
                }
            }
            object Number()
            {
                string word=Word();
                if(word.IndexOf('.')==-1){long.TryParse(word,out long value);return value;}
                double.TryParse(word,out double number);return number;
            }
            string String()
            {
                var s=new StringBuilder();reader.Read();
                while(reader.Peek()!=-1)
                {
                    char c=Next;if(c=='"')break;
                    if(c!='\\'){s.Append(c);continue;}
                    if(reader.Peek()==-1)break;
                    c=Next;
                    switch(c)
                    {
                        case '"':case '\\':case '/':s.Append(c);break;
                        case 'b':s.Append('\b');break;case 'f':s.Append('\f');break;
                        case 'n':s.Append('\n');break;case 'r':s.Append('\r');break;case 't':s.Append('\t');break;
                        case 'u':var hex=new char[4];for(int i=0;i<4;i++)hex[i]=Next;s.Append((char)Convert.ToInt32(new string(hex),16));break;
                    }
                }
                return s.ToString();
            }
        }
    }
}
