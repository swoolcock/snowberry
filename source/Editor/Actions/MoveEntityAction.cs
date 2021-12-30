using Microsoft.Xna.Framework;

namespace Snowberry.Editor.Actions {
    public class MoveEntityAction : EditorAction<Vector2> {
        public Entity Entity { get; }

        public MoveEntityAction(Entity entity, Vector2 oldValue, Vector2 newValue)
            : base(oldValue, newValue) {
            Entity = entity;
            Child = new MoveEntityNodesAction(Entity, this.Diff());
        }

        public MoveEntityAction(Entity entity, Vector2 offset)
            : this(entity, Vector2.Zero, offset) {
        }

        public override void Apply() {
            Entity.Move(this.Diff());
            base.Apply();
        }

        public override void Unapply() {
            base.Unapply();
            Entity.Move(-this.Diff());
        }
    }
}