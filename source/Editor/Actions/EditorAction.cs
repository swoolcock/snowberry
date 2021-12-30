using System;
using System.Collections.Generic;
using System.Linq;
using Celeste.Mod;
using Microsoft.Xna.Framework;

namespace Snowberry.Editor.Actions {
    public class EditorAction {
        private EditorAction[] children = new EditorAction[0];
        public IEnumerable<EditorAction> Children {
            get => children;
            protected set => children = value?.ToArray() ?? new EditorAction[0];
        }

        public EditorAction Child {
            get => children.FirstOrDefault();
            protected set => children = value == null ? new EditorAction[0] : new[] { value };
        }

        public Action ApplyAction { get; set; }
        public Action UnapplyAction { get; set; }

        public EditorAction(IEnumerable<EditorAction> children = null) {
            Children = children;
        }

        public virtual void Apply() {
            ApplyAction?.Invoke();
            foreach (var child in Children) {
                child.Apply();
            }
        }

        public virtual void Unapply() {
            foreach (var child in Children) {
                child.Unapply();
            }
            UnapplyAction?.Invoke();
        }
    }

    public abstract class EditorAction<TValue> : EditorAction {
        public TValue OldValue { get; }
        public TValue NewValue { get; }

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