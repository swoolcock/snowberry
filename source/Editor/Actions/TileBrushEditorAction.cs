using Microsoft.Xna.Framework;

namespace Snowberry.Editor.Actions {
    public class TileBrushEditorAction : EditorAction<string> {
        public readonly Room Room;
        public readonly bool ForegroundLayer;
        public readonly Rectangle DirtyRegion;

        public TileBrushEditorAction(Room room, bool foregroundLayer, Rectangle dirtyRegion, string oldValue, string newValue)
            : base(oldValue, newValue) {
            Room = room;
            ForegroundLayer = foregroundLayer;
            DirtyRegion = dirtyRegion;
        }

        public override void Apply() => Room.SetTiles(ForegroundLayer, DirtyRegion, NewValue, true);
        public override void Unapply() => Room.SetTiles(ForegroundLayer, DirtyRegion, OldValue, true);
    }
}