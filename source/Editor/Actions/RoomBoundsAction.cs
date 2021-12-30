using System.Linq;
using Microsoft.Xna.Framework;

namespace Snowberry.Editor.Actions {
    public class RoomBoundsAction : EditorAction<Rectangle> {
        protected readonly Room Room;

        public RoomBoundsAction(Room room, Rectangle oldValue, Rectangle newValue)
            : base(oldValue, newValue) {
            Room = room;

            var positionDiff = this.PositionDiff();
            if (positionDiff != Vector2.Zero) {
                Children = Room.AllEntities.Select(entity => new MoveEntityAction(entity, positionDiff));
            }
        }

        public override void Apply() {
            Room.Bounds = NewValue;
            base.Apply();
        }

        public override void Unapply() {
            base.Unapply();
            Room.Bounds = OldValue;
        }
    }
}