namespace Snowberry.Editor.Actions {
    public class AddEntityAction : EditorAction {
        public Entity Entity { get; }
        protected Room Room { get; }

        public AddEntityAction(Entity entity) {
            Entity = entity;
            Room = entity.Room;
        }

        public override void Apply() {
            Room.AddEntity(Entity);
            base.Apply();
        }

        public override void Unapply() {
            base.Unapply();
            Room.RemoveEntity(Entity);
        }
    }
}