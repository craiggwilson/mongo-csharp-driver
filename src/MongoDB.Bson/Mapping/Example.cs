using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Mapping.Configuration;
using MongoDB.Bson.Mapping.Configuration.Conventions;

namespace MongoDB.Bson.Mapping
{
    public class Example
    {
        public void AtStartup()
        {
            var config = new MappingConfiguration();

            // add or remove conventions
            config.Conventions.Add(new CamelCaseElementNameConvention());

            // only run conventions
            config.Map<Person>();

            // manual mapping
            config.Map<Pet>(pet =>
            {
                pet.Map<string>("Name").ElementName("name");
            });

            // should pick up the below JobClassModel<Job> and any others in that assembly
            config.ScanAssemblyOf<JobClassModel<Job>>();
        }

        // manual mapping in a separate file for each entity (or something like that)
        private class JobClassModel<Job> : ClassModel<Job>
        {
            public JobClassModel()
            {
                Map<string>("Name").ElementName("nm");
            }
        }



        private class Person
        {
            public string FirstName { get; set; }

            public string LastName { get; set; }

            public List<Pet> Pets { get; set; }
        }

        private class Pet
        {
            public string Name { get; set; }
        }

        private class Job
        {
            public string Name { get; set; }
        }

    }
}
