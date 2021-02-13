using System;
using System.Linq;

namespace SimpleParser
{
    class Program
    {
        static void Main(string[] args)
        {
            var dataForSplit = "{\"id\":1,\"name\":\"data\",\"gavno\":{\"id\":1,\"name\":\"data\"}}";
            var data = JsonParser.Parse<ParseObject>(dataForSplit);
        }
    }
}
