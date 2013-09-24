using System;
using System.Linq;
using GeekNight.Models;
using GeekNight.Relationships;
using NUnit.Framework;
using Neo4jClient;
using Neo4jClient.Cypher;

namespace GeekNight.Test
{
    [TestFixture]
    public class Model1Test
    {
        GraphClient client;

        [SetUp]
        public void Setup()
        {
            client = new GraphClient(new Uri("http://localhost:7474/db/data"));
            client.Connect();
            //BuildGraph(); 
        }


        /*
       firswood -1-> old trafford -2-> cornbrook -4-> city centre --
                                         |                          |
                                         |                          /
                                          -8-> media city <-16---- /
       */
        private void BuildGraph()
        {
            client.CreateIndex("tramStations", new IndexConfiguration() { Provider = IndexProvider.lucene, Type = IndexType.fulltext }, IndexFor.Node);

            var mediaCity = CreateStop("Media City");
            var firswood = CreateStop("Firswood");
            var oldTrafford = CreateStop("Old Trafford");
            var cornbrook = CreateStop("Cornbrook");
            var cityCentre = CreateStop("City Centre");

            client.CreateRelationship(firswood, new GoesTo(oldTrafford, new {duration="1"}));
            client.CreateRelationship(oldTrafford, new GoesTo(cornbrook, new {duration="2"}));
            client.CreateRelationship(cornbrook, new GoesTo(cityCentre, new {duration="4"}));
            client.CreateRelationship(cornbrook, new GoesTo(mediaCity, new {duration="8"}));
            client.CreateRelationship(cityCentre, new GoesTo(mediaCity, new {duration="16"}));


        }


        [Test]
        public void ShouldGetFirsWoodFromIndex()
        {

            var query = client
                .Cypher
                .Start(new {startNode = Node.ByIndexLookup("Stops","Name","Firswood")})
                .Return<Node<Stop>>("startNode");

            var result = query.Results.First().Data;

            Assert.That(result.Name, Is.EqualTo("Firswood"));

        }

        [Test]
        public void shouldFindTheQuickestPathBetweenFirswoodAndMediaCity()
        {

            var query = client
                      .Cypher
                      .Start(new { startNode = Node.ByIndexLookup("Stops", "Name", "Firswood") ,
                                   endNode = Node.ByIndexLookup("Stops", "Name", "Media City")
                      })
                      .Match("p=startNode-[:GOES_TO*]->endNode")
                      .Return<Node<Stop>>("p");

            var result = query.Results;

            Assert.That(result.Count(), Is.EqualTo(0));



//            var query = client
//      .Cypher
//      .Start(new { startNode = new IndexEntry("tramStations").Name = }  )
//      .Match("startNode-[:GOES_TO*]->endNode")
//      //.Where((Book bk) => bk.Pages > 5)
//      .Return(()=>All.Nodes);
//
//            var longBooks = query.Results;



            Assert.That(query.Results.Count(), Is.GreaterThan(0));
        }

        private NodeReference<Stop> CreateStop(string stopName)
        {
            var stop = new Stop() {Name = stopName};

            var node = client.Create(stop,
                                                 new IRelationshipAllowingParticipantNode<Stop>[0],
                                                 new[]
                                                     {
                                                         new IndexEntry("tramStations")
                                                             {
                                                                 {"Name", stop.Name}
                                                             }
                                                     });
            return node;
        }
    }
}
