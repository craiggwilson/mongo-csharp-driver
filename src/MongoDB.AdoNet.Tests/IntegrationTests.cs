using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace MongoDB.AdoNet.Tests
{
    [TestFixture]
    public class IntegrationTests
    {
        [Test]
        public void Test()
        {
            using(var connection = new MongoConnection("mongodb://localhost/test"))
            {
                connection.Open();

                var cmd = connection.CreateCommand();
                cmd.CommandText = @"
                    FROM person 
                    MATCH { Name: /^J/ } 
                    PROJECT {Name: 1, _id: 0}";

                using(var reader = cmd.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        var nameOrdinal = reader.GetOrdinal("Name");
                        Console.WriteLine("Name = {0}", reader.GetString(nameOrdinal));
                    }
                }
            }
        }
    }
}