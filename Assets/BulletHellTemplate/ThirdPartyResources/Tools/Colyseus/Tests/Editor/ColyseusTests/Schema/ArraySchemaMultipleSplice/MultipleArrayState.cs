
using Colyseus.Schema;

namespace SchemaTest.ArraySchemaMultipleSplice {
	public partial class MultipleArraySpliceState : Schema {
		[Type(0, "ref", typeof(Player))]
		public Player player = new Player();
	}
}
