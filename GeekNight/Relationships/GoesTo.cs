using System;
using GeekNight.Models;
using Neo4jClient;

namespace GeekNight.Relationships
{
    public class GoesTo : Relationship, IRelationshipAllowingParticipantNode<Stop>, IRelationshipAllowingSourceNode<Stop>
    {
        public static readonly string TypeKey = "Goes_To";


        public GoesTo(NodeReference targetNode) : base(targetNode)
        {
        }

        public GoesTo(NodeReference targetNode, object data) : base(targetNode, data)
        {
        }

        public override string RelationshipTypeKey
        {
            get { return TypeKey; }
        }
    }
}