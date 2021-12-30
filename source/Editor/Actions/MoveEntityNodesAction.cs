using System.Linq;
using Microsoft.Xna.Framework;

namespace Snowberry.Editor.Actions {
    public class MoveEntityNodesAction : EditorAction<Vector2> {
        public Entity Entity { get; }
        public int[] Nodes { get; }

        public MoveEntityNodesAction(Entity entity, Vector2 oldValue, Vector2 newValue, params int[] nodes)
            : base(oldValue, newValue) {
            Entity = entity;
            Nodes = nodes.Any() ? nodes : Enumerable.Range(0, Entity.Nodes.Length).ToArray();
        }

        public MoveEntityNodesAction(Entity entity, Vector2 offset, params int[] nodes)
            : this(entity, Vector2.Zero, offset, nodes) {
        }

        public override void Apply() {
            var diff = this.Diff();
            foreach (var index in Nodes) {
                Entity.MoveNode(index, diff);
            }
            base.Apply();
        }

        public override void Unapply() {
            base.Unapply();
            var diff = this.Diff();
            foreach (var index in Nodes) {
                Entity.MoveNode(index, -diff);
            }
        }
    }
}