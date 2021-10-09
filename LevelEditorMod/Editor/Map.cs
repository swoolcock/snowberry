﻿using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace LevelEditorMod.Editor {
    public class Map {
        private readonly List<Room> rooms = new List<Room>();
        private readonly List<Rectangle> fillers = new List<Rectangle>();

        public readonly string Name;

        internal Map(string name) {
            Name = name;
        }

        internal Map(MapData data)
            : this(data.Filename) {
            foreach (LevelData roomData in data.Levels)
                rooms.Add(new Room(roomData));
            foreach (Rectangle filler in data.Filler)
                fillers.Add(filler);
        }

        internal void Render(Rectangle viewRect) {
			Draw.Line(new Vector2(0, viewRect.Top), new Vector2(0, viewRect.Bottom), Color.White * 0.01f, 2);
			Draw.Line(new Vector2(viewRect.Left, 0), new Vector2(viewRect.Right, 0), Color.White * 0.01f, 2);

			foreach (Room room in rooms) {
				Rectangle rect = new Rectangle(room.Bounds.X * 8, room.Bounds.Y * 8, room.Bounds.Width * 8, room.Bounds.Height * 8);
				if (!viewRect.Intersects(rect))
					continue;
				room.Render(viewRect);
			}

			foreach (Rectangle filler in fillers) {
				Rectangle rect = new Rectangle(filler.X * 8, filler.Y * 8, filler.Width * 8, filler.Height * 8);
				Draw.Rect(rect, Color.White * 0.08f);
				Draw.HollowRect(rect, Color.OrangeRed);
			}
		}
    }
}