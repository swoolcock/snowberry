using System.Collections.Generic;
using System.Linq;
using Monocle;
using Snowberry.Editor.Actions;

namespace Snowberry.Editor {
    public class UndoRedoStack {
        private LinkedList<EditorAction> undoActions = new();
        public IEnumerable<EditorAction> UndoActions => undoActions;

        private readonly LinkedList<EditorAction> redoActions = new();
        public IEnumerable<EditorAction> RedoActions => redoActions;

        private int maxUndoCount = 50;
        public int MaxUndoCount {
            get => maxUndoCount;
            set {
                maxUndoCount = Calc.Clamp(value, 0, 100);
                ensureUndoCount();
            }
        }

        public bool CanUndo() => undoActions.Any();
        public bool CanRedo() => redoActions.Any();

        public bool Undo() {
            if (!CanUndo()) return false;
            Editor.SelectedEntities = null;
            var action = undoActions.First.Value;
            undoActions.RemoveFirst();
            redoActions.AddFirst(action);
            action.Unapply();
            return true;
        }

        public bool Redo() {
            if (!CanRedo()) return false;
            Editor.SelectedEntities = null;
            var action = redoActions.First.Value;
            redoActions.RemoveFirst();
            undoActions.AddFirst(action);
            action.Apply();
            return true;
        }

        public void PushAction(EditorAction action, bool apply = false) {
            redoActions.Clear();
            undoActions.AddFirst(action);
            ensureUndoCount();
            if (apply) action.Apply();
        }

        private void ensureUndoCount() {
            if (undoActions.Count > MaxUndoCount) {
                undoActions = new LinkedList<EditorAction>(undoActions.Take(MaxUndoCount));
            }
        }
    }
}