﻿using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;
using static Celeste.TrackSpinner;

namespace Snowberry.Editor.Entities {
    [Plugin("spinner")]
    public class Plugin_Spinner : Entity {
        [Option("attachToSolid")] public bool Attached = false;
        [Option("color")] public CrystalColor SpinnerColor = CrystalColor.Blue;
        [Option("dust")] public bool Dust = false;

        private List<Entity> connectTo;

        public Plugin_Spinner() {
            Tracked = true;
        }

		public override void RenderBefore() {
			base.RenderBefore();
            if(Editor.FancyRender && !(Dust || IsVanillaDust())) {
                CrystalColor color = GetColorForVanillaMap() ?? SpinnerColor;

                var colourString = color switch {
                    CrystalColor.Blue => "blue",
                    CrystalColor.Red => "red",
                    CrystalColor.Purple => "purple",
                    _ => "white"
                };

                Color c = Color.White;
                if(color == CrystalColor.Rainbow) {
                    c = Calc.HsvToColor(0.4f + Calc.YoYo(Position.Length() % 280 / 280) * 0.4f, 0.4f, 0.9f);
                }

                UpdateConnections();
                MTexture bg = GFX.Game[$"danger/crystal/bg_{colourString}00"];
                foreach(var item in connectTo)
                    bg.DrawCentered(Position + (item.Position - Position) / 2, c);
            }
        }

		public override void Render() {
            base.Render();
            
            CrystalColor color = GetColorForVanillaMap() ?? SpinnerColor;

            if (Dust || IsVanillaDust()) {
                GFX.Game["danger/dustcreature/base00"].DrawCentered(Position);
                GFX.Game["danger/dustcreature/center00"].DrawCentered(Position);
            } else {
                var colourString = color switch {
                    CrystalColor.Blue => "blue",
                    CrystalColor.Red => "red",
                    CrystalColor.Purple => "purple",
                    _ => "white"
                };
                Color c = Color.White;
                if(color == CrystalColor.Rainbow)
                    c = Calc.HsvToColor(0.4f + Calc.YoYo(Position.Length() % 280 / 280) * 0.4f, 0.4f, 0.9f);
                MTexture spinner = GFX.Game[$"danger/crystal/fg_{colourString}03"];
                spinner.DrawCentered(Position, c);
            }
        }

        private void UpdateConnections() {
			if(connectTo == null || Room.DirtyTrackedEntities.ContainsKey(typeof(Plugin_Spinner)) && Room.DirtyTrackedEntities[typeof(Plugin_Spinner)]) {
                connectTo = new List<Entity>();
				foreach(var item in Room.TrackedEntities[typeof(Plugin_Spinner)]) {
					if((item.Position - Position).LengthSquared() < 24 * 24) {
                        connectTo.Add(item);
					}
				}
            }
        }

        public static void AddPlacements() {
            string[] types = new string[] { "Blue", "Red", "Purple", "Rainbow" };
            foreach(var type in types)
                Placements.Create($"Spinner ({type})", "spinner", new Dictionary<string, object>() { { "color", type } });
        }

        public CrystalColor? GetColorForVanillaMap() {
            if(Editor.GetCurrent() == null || Editor.GetCurrent().Map == null)
                return null;
            return Editor.GetCurrent().Map.From.ID switch {
                5 => CrystalColor.Red,
                6 => CrystalColor.Purple,
                10 => CrystalColor.Rainbow,
                _ => null
            };
        }

        public bool IsVanillaDust() {
            if(Editor.GetCurrent() == null || Editor.GetCurrent().Map == null || Room == null && Editor.SelectedRoom == null)
                return false;
            AreaKey area = Editor.GetCurrent().Map.From;
            return area.ID == 3 || (area.ID == 7 && ((Room ?? Editor.SelectedRoom)?.Name?.StartsWith("d-") ?? false));
        }
    }

    public class Plugin_MovingSpinner : Entity {
        [Option("dust")] public bool Dust = false;
        [Option("star")] public bool Star = false;

        public override void Render() {
            base.Render();

            Vector2 stop = Nodes[0];

            if (Star) {
                MTexture star = GFX.Game["danger/starfish13"];
                star.DrawCentered(Position);
                star.DrawCentered(stop);
            } else if (Dust) {
                MTexture dustbase = GFX.Game["danger/dustcreature/base00"],
                    dustcenter = GFX.Game["danger/dustcreature/center00"];
                dustbase.DrawCentered(Position);
                dustcenter.DrawCentered(Position);
                dustbase.DrawCentered(stop);
                dustcenter.DrawCentered(stop);
            } else {
                MTexture blade = GFX.Game["danger/blade00"];
                blade.DrawCentered(Position);
                blade.DrawCentered(stop);
            }
        }

        public override void ApplyDefaults() {
            base.ChangeDefault();
            ResetNodes();
            AddNode(Position + new Vector2(16, 0));
        }
    }

    [Plugin("trackSpinner")]
    public class Plugin_TrackSpinner : Plugin_MovingSpinner {
        [Option("speed")] public Speeds Speed = Speeds.Normal;
        [Option("startCenter")] public bool StartAtCenter = false;

        public override void Render() {
            DrawUtil.DottedLine(Position, Nodes[0], Color.White * 0.5f, 8, 4);
            base.Render();
        }
    }

    [Plugin("rotateSpinner")]
    public class Plugin_RotateSpinner : Plugin_MovingSpinner {
        [Option("clockwise")] public bool Clockwise = false;

        public override void Render() {
            Draw.Circle(Position, Vector2.Distance(Position, Nodes[0]), Color.White * 0.5f, 20);
            base.Render();
        }
    }
}
