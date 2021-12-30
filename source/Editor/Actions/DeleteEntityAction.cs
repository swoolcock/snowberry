namespace Snowberry.Editor.Actions {
    public class DeleteEntityAction : EditorAction {
        public Entity Entity { get; }
        protected Room Room { get; }

        public DeleteEntityAction(Entity entity) {
            Entity = entity;
            Room = entity.Room;
        }

        public override void Apply() {
            Room.RemoveEntity(Entity);
            base.Apply();
        }

        public override void Unapply() {
            base.Unapply();
            Room.AddEntity(Entity);
        }
    }
}