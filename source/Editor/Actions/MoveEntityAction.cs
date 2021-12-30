using Microsoft.Xna.Framework;

namespace Snowberry.Editor.Actions {
    public class MoveEntityAction : EditorAction<Vector2> {
        public Entity Entity { get; }

        public MoveEntityAction(Entity entity, Vector2 oldValue, Vector2 newValue, bool moveNodes = true)
            : base(oldValue, newValue) {
            Entity = entity;
            if (moveNodes) {
                Child = new MoveEntityNodesAction(Entity, this.Diff());
            }
        }

        public MoveEntityAction(Entity entity, Vector2 offset, bool moveNodes = true)
            : this(entity, Vector2.Zero, offset, moveNodes) {
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