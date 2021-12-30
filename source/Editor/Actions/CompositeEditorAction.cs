using System.Collections.Generic;
using System.Linq;
using Celeste.Mod;
using Microsoft.Xna.Framework;

namespace Snowberry.Editor.Actions {
    public class CompositeEditorAction : EditorAction {
        private EditorAction[] actions = new EditorAction[0];
        public IEnumerable<EditorAction> Actions {
            get => actions;
            set => actions = value?.ToArray() ?? new EditorAction[0];
        }

        public CompositeEditorAction(IEnumerable<EditorAction> actions = null) {
            Actions = actions;
        }

        public override void Apply() {
            foreach (var action in Actions) {
                action.Apply();
            }
        }

        public override void Unapply() {
            foreach (var action in Actions.Reverse()) {
                action.Unapply();
            }
        }
    }

    public abstract class CompositeEditorAction<TValue> : CompositeEditorAction {
        public TValue OldValue { get; set; }
        public TValue NewValue { get; set; }

        protected CompositeEditorAction(TValue oldValue, TValue newValue, IEnumerable<EditorAction> actions = null)
            : base(actions) {
            OldValue = oldValue;
            NewValue = newValue;
        }
    }

    public static class CompositeEditorActionExtensions {
        public static Vector2 Diff(this CompositeEditorAction<Vector2> self) => self.NewValue - self.OldValue;
        public static int Diff(this CompositeEditorAction<int> self) => self.NewValue - self.OldValue;
        public static float Diff(this CompositeEditorAction<float> self) => self.NewValue - self.OldValue;
        public static Vector2 PositionDiff(this CompositeEditorAction<Rectangle> self) => (self.NewValue.Location - self.OldValue.Location).ToVector2();
        public static Vector2 SizeDiff(this CompositeEditorAction<Rectangle> self) => new Vector2(self.NewValue.Width - self.OldValue.Width, self.NewValue.Height - self.OldValue.Height);
    }
}