using Celeste.Mod;
using Microsoft.Xna.Framework;

namespace Snowberry.Editor.Actions {
    public abstract class EditorAction {
        public abstract void Apply();
        public abstract void Unapply();
    }

    public abstract class EditorAction<TValue> : EditorAction {
        public TValue OldValue { get; set; }
        public TValue NewValue { get; set; }

        protected EditorAction(TValue oldValue, TValue newValue) {
            OldValue = oldValue;
            NewValue = newValue;
        }
    }

    public static class EditorActionExtensions {
        public static Vector2 Diff(this EditorAction<Vector2> self) => self.NewValue - self.OldValue;
        public static int Diff(this EditorAction<int> self) => self.NewValue - self.OldValue;
        public static float Diff(this EditorAction<float> self) => self.NewValue - self.OldValue;
        public static Vector2 PositionDiff(this EditorAction<Rectangle> self) => (self.NewValue.Location - self.OldValue.Location).ToVector2();
        public static Vector2 SizeDiff(this EditorAction<Rectangle> self) => new Vector2(self.NewValue.Width - self.OldValue.Width, self.NewValue.Height - self.OldValue.Height);
    }
}