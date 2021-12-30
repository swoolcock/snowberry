﻿using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Monocle;
using Snowberry.Editor.Triggers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Snowberry.Editor {
    using Element = BinaryPacker.Element;

    public class Room {
        public string Name;

        public Rectangle Bounds;

        public Map Map { get; private set; }

        public int X => Bounds.X;
        public int Y => Bounds.Y;
        public int Width => Bounds.Width;
        public int Height => Bounds.Height;
        public Vector2 Position => new Vector2(X, Y);
        public Vector2 Size => new Vector2(Width, Height);

        public Rectangle ScissorRect { get; private set; }

        // Music data
        public string Music = "";
        public string AltMusic = "";
        public string Ambience = "";
        public bool[] MusicLayers = new bool[4];

        public int MusicProgress;
        public int AmbienceProgress;

        // Camera offset data
        public Vector2 CameraOffset;

        // Misc data
        public bool Dark;
        public bool Underwater;
        public bool Space;
        public WindController.Patterns WindPattern = WindController.Patterns.None;

        // Tiles
        private VirtualMap<char> fgTileMap;
        private VirtualMap<char> bgTileMap;
        private VirtualMap<MTexture> fgTiles, bgTiles;

        public readonly List<Decal> FgDecals = new List<Decal>();
        public readonly List<Decal> BgDecals = new List<Decal>();

        public readonly List<Entity> Entities = new List<Entity>();
        public readonly List<Entity> Triggers = new List<Entity>();
        public readonly List<Entity> AllEntities = new List<Entity>();

        public readonly Dictionary<Type, List<Entity>> TrackedEntities = new Dictionary<Type, List<Entity>>();
        public readonly Dictionary<Type, bool> DirtyTrackedEntities = new Dictionary<Type, bool>();

        public int LoadSeed {
            get {
                int num = 0;
                string name = Name;
                foreach (char c in name) {
                    num += c;
                }

                return num;
            }
        }

        private static readonly Regex tileSplitter = new Regex("\\r\\n|\\n\\r|\\n|\\r");

        internal Room(string name, Rectangle bounds) {
            Name = name;
            Bounds = bounds;
            fgTileMap = new VirtualMap<char>(bounds.Width, bounds.Height, '0');
            bgTileMap = new VirtualMap<char>(bounds.Width, bounds.Height, '0');
            Autotile();
        }

        internal Room(LevelData data, Map map)
            : this(data.Name, data.TileBounds) {
            Map = map;

            // Music
            Music = data.Music;
            AltMusic = data.AltMusic;
            Ambience = data.Ambience;

            MusicLayers = new bool[4];
            MusicLayers[0] = data.MusicLayers[0] > 0;
            MusicLayers[1] = data.MusicLayers[1] > 0;
            MusicLayers[2] = data.MusicLayers[2] > 0;
            MusicLayers[3] = data.MusicLayers[3] > 0;

            MusicProgress = data.MusicProgress;
            AmbienceProgress = data.AmbienceProgress;

            // Camera
            CameraOffset = data.CameraOffset;

            // Misc
            Dark = data.Dark;
            Underwater = data.Underwater;
            Space = data.Space;
            WindPattern = data.WindPattern;

            // BgTiles
            string[] array = tileSplitter.Split(data.Bg);
            for (int i = 0; i < array.Length; i++) {
                for (int j = 0; j < array[i].Length; j++) {
                    bgTileMap[j, i] = array[i][j];
                }
            }

            // FgTiles
            string[] array2 = tileSplitter.Split(data.Solids);
            for (int i = 0; i < array2.Length; i++) {
                for (int j = 0; j < array2[i].Length; j++) {
                    fgTileMap[j, i] = array2[i][j];
                }
            }

            Autotile();

            // BgDecals
            foreach (DecalData decal in data.BgDecals) {
                BgDecals.Add(new Decal(this, decal));
            }

            // FgDecals
            foreach (DecalData decal in data.FgDecals) {
                FgDecals.Add(new Decal(this, decal));
            }

            // Entities
            foreach (EntityData entity in data.Entities) {
                if (Entity.TryCreate(this, entity, out Entity e)) {
                    AddEntity(e);
                } else
                    Snowberry.Log(LogLevel.Warn, $"Attempted to load unknown entity ('{entity.Name}')");
            }

            // Player Spawnpoints (excluded from LevelData.Entities)
            foreach (Vector2 spawn in data.Spawns) {
                var spawnEntity = Entity.Create("player", this).SetPosition(spawn);
                AddEntity(spawnEntity);
            }

            // Triggers
            foreach (EntityData trigger in data.Triggers) {
                if (Entity.TryCreate(this, trigger, out Entity t)) {
                    AddEntity(t);
                } else
                    Snowberry.Log(LogLevel.Warn, $"Attempted to load unknown trigger ('{trigger.Name}')");
            }
        }

        #region Tilemap Helpers

        public char GetFgTileWorld(Vector2 at) => GetTileWorld(true, at);
        public char GetBgTileWorld(Vector2 at) => GetTileWorld(false, at);
        public bool SetFgTileWorld(Vector2 at, char tile) => SetTileWorld(true, at, tile);
        public bool SetBgTileWorld(Vector2 at, char tile) => SetTileWorld(false, at, tile);
        public char GetFgTile(int x, int y) => GetTile(true, x, y);
        public char GetBgTile(int x, int y) => GetTile(false, x, y);
        public bool SetFgTile(int x, int y, char tile) => SetTile(true, x, y, tile);
        public bool SetBgTile(int x, int y, char tile) => SetTile(false, x, y, tile);
        public string GetFgTilesWorld(Rectangle at) => GetTilesWorld(true, at);
        public string GetBgTilesWorld(Rectangle at) => GetTilesWorld(false, at);
        public bool SetFgTilesWorld(Rectangle at, string tiles) => SetTilesWorld(true, at, tiles);
        public bool SetBgTilesWorld(Rectangle at, string tiles) => SetTilesWorld(false, at, tiles);
        public string GetFgTiles(Rectangle at) => GetTiles(true, at);
        public string GetBgTiles(Rectangle at) => GetTiles(false, at);
        public bool SetFgTiles(Rectangle at, string tiles) => SetTiles(true, at, tiles);
        public bool SetBgTiles(Rectangle at, string tiles) => SetTiles(false, at, tiles);
        public char GetTileWorld(bool fg, Vector2 at) => GetTile(fg, (int)(at.X - Position.X), (int)(at.Y - Position.Y));
        public bool SetTileWorld(bool fg, Vector2 at, char tile) => SetTile(fg, (int)(at.X - Position.X), (int)(at.Y - Position.Y), tile);
        public string GetTilesWorld(bool fg, Rectangle at) => GetTiles(fg, new Rectangle((int)(at.Location.X + Position.X), (int)(at.Location.Y + Position.Y), at.Width, at.Height));
        public bool SetTilesWorld(bool fg, Rectangle at, string tiles) => SetTiles(fg, new Rectangle((int)(at.Location.X + Position.X), (int)(at.Location.Y + Position.Y), at.Width, at.Height), tiles);

        public char GetTile(bool fg, int x, int y) => (fg ? fgTileMap : bgTileMap)[x, y];

        public bool SetTile(bool fg, int x, int y, char tile, bool autotile = false) {
            var tileMap = fg ? fgTileMap : bgTileMap;
            if (tileMap[x, y] == tile) return false;
            tileMap[x, y] = tile;
            if (autotile) Autotile();
            return true;
        }

        public string GetTiles(bool fg, Rectangle at) {
            var tileMap = fg ? fgTileMap : bgTileMap;
            StringBuilder sb = new StringBuilder(at.Width * at.Height);
            for (int y = 0; y < at.Height; y++) {
                for (int x = 0; x < at.Width; x++) {
                    sb.Append(tileMap[at.X + x, at.Y + y]);
                }
            }
            return sb.ToString();
        }

        public bool SetTiles(bool fg, Rectangle at, string tiles, bool autotile = false) {
            var rv = false;
            for (int y = 0; y < at.Height; y++) {
                for (int x = 0; x < at.Width; x++) {
                    rv |= SetTile(fg, at.X + x, at.Y + y, tiles[x + y * at.Width]);
                }
            }
            if (rv && autotile) Autotile();
            return rv;
        }

        #endregion

        public void Autotile() {
            fgTiles = GFX.FGAutotiler.GenerateMap(fgTileMap, new Autotiler.Behaviour() { EdgesExtend = true }).TileGrid.Tiles;
            bgTiles = GFX.BGAutotiler.GenerateMap(bgTileMap, new Autotiler.Behaviour() { EdgesExtend = true }).TileGrid.Tiles;
        }

        internal List<EntitySelection> GetSelectedEntities(Rectangle rect) {
            List<EntitySelection> result = new List<EntitySelection>();

            foreach (Entity entity in AllEntities) {
                var rects = entity.SelectionRectangles;
                if (rects != null && rects.Length > 0) {
                    List<EntitySelection.Selection> selection = new List<EntitySelection.Selection>();
                    bool wasSelected = false;
                    for (int i = 0; i < rects.Length; i++) {
                        Rectangle r = rects[i];
                        if (rect.Intersects(r)) {
                            selection.Add(new EntitySelection.Selection(r, i - 1));
                            wasSelected = true;
                        }
                    }

                    if (wasSelected)
                        result.Add(new EntitySelection(entity, selection));
                }
            }

            return result;
        }

        internal void CalculateScissorRect(Editor.BufferCamera camera) {
            Vector2 offset = Position * 8;

            Vector2 zero = Calc.Round(Vector2.Transform(offset, camera.Matrix));
            Vector2 size = Calc.Round(Vector2.Transform(offset + new Vector2(Width * 8, Height * 8), camera.Matrix) - zero);
            ScissorRect = new Rectangle(
                (int)zero.X, (int)zero.Y,
                (int)size.X, (int)size.Y);
        }

        internal void Render(Rectangle viewRect) {
            Vector2 offset = Position * 8;

            Draw.Rect(offset, Width * 8, Height * 8, Color.White * 0.1f);

            int startX = Math.Max(0, (viewRect.Left - X * 8) / 8);
            int startY = Math.Max(0, (viewRect.Top - Y * 8) / 8);
            int endX = Math.Min(Width, Width + (viewRect.Right - (X + Width) * 8) / 8);
            int endY = Math.Min(Height, Height + (viewRect.Bottom - (Y + Height) * 8) / 8);

            // BgTiles
            for (int x = startX; x < endX; x++)
                for (int y = startY; y < endY; y++)
                    if (bgTiles[x, y] != null)
                        bgTiles[x, y].Draw(offset + new Vector2(x, y) * 8);

            // BgDecals
            foreach (Decal decal in BgDecals)
                decal.Render(offset);

            // Entities
            foreach (Entity entity in Entities) {
                Calc.PushRandom(entity.GetHashCode());
                entity.RenderBefore();
                Calc.PopRandom();
            }

            foreach (Entity entity in Entities) {
                Calc.PushRandom(entity.GetHashCode());
                entity.Render();
                Calc.PopRandom();
            }

            // FgTiles
            for (int x = startX; x < endX; x++)
                for (int y = startY; y < endY; y++)
                    if (fgTiles[x, y] != null)
                        fgTiles[x, y].Draw(offset + new Vector2(x, y) * 8);

            // FgDecals
            foreach (Decal decal in FgDecals)
                decal.Render(offset);

            // Triggers
            foreach (Entity trigger in Triggers)
                trigger.Render();

            if (this == Editor.SelectedRoom) {
                if (Editor.Selection.HasValue)
                    Draw.Rect(Editor.Selection.Value, Color.Blue * 0.25f);
                if (Editor.SelectedEntities != null) {
                    foreach (EntitySelection s in Editor.SelectedEntities) {
                        foreach (EntitySelection.Selection selection in s.Selections) {
                            Draw.Rect(selection.Rect, Color.Blue * 0.25f);
                        }
                    }
                }
            } else
                Draw.Rect(offset, Width * 8, Height * 8, Color.Black * 0.5f);

            DirtyTrackedEntities.Clear();
        }

        internal void HQRender(Matrix m) {
            Draw.SpriteBatch.GraphicsDevice.ScissorRectangle = ScissorRect;

            // Entities
            foreach (Entity entity in Entities)
                entity.HQRender();
            // Triggers
            foreach (Entity trigger in Triggers)
                trigger.HQRender();
        }

        public void UpdateBounds() {
            var newFgTiles = new VirtualMap<char>(Bounds.Width, Bounds.Height, '0');
            for (int x = 0; x < fgTileMap.Columns; x++)
                for (int y = 0; y < fgTileMap.Rows; y++)
                    newFgTiles[x, y] = fgTileMap[x, y];
            fgTileMap = newFgTiles;

            var newBgTiles = new VirtualMap<char>(Bounds.Width, Bounds.Height, '0');
            for (int x = 0; x < bgTileMap.Columns; x++)
                for (int y = 0; y < bgTileMap.Rows; y++)
                    newBgTiles[x, y] = bgTileMap[x, y];
            bgTileMap = newBgTiles;

            Autotile();
        }

        public Element CreateLevelData() {
            Element ret = new Element {
                Attributes = new Dictionary<string, object> {
                    ["name"] = "lvl_" + Name,
                    ["x"] = X * 8,
                    ["y"] = Y * 8,
                    ["width"] = Width * 8,
                    ["height"] = Height * 8,

                    ["music"] = Music,
                    ["alt_music"] = AltMusic,
                    ["ambience"] = Ambience,
                    ["musicLayer1"] = MusicLayers[0],
                    ["musicLayer2"] = MusicLayers[1],
                    ["musicLayer3"] = MusicLayers[2],
                    ["musicLayer4"] = MusicLayers[3],

                    ["musicProgress"] = MusicProgress,
                    ["ambienceProgress"] = AmbienceProgress,

                    ["dark"] = Dark,
                    ["underwater"] = Underwater,
                    ["space"] = Space,
                    ["windPattern"] = WindPattern.ToString(),

                    ["cameraOffsetX"] = CameraOffset.X,
                    ["cameraOffsetY"] = CameraOffset.Y
                }
            };

            Element entitiesElement = new Element {
                Attributes = new Dictionary<string, object>(),
                Name = "entities",
                Children = new List<Element>()
            };
            ret.Children = new List<Element>();
            ret.Children.Add(entitiesElement);

            foreach (var entity in Entities) {
                Element entityElem = new Element {
                    Name = entity.Name,
                    Children = new List<Element>(),
                    Attributes = new Dictionary<string, object> {
                        ["id"] = entity.EntityID,
                        ["x"] = entity.X - X * 8,
                        ["y"] = entity.Y - Y * 8,
                        ["width"] = entity.Width,
                        ["height"] = entity.Height,
                        ["originX"] = entity.Origin.X,
                        ["originY"] = entity.Origin.Y
                    }
                };

                foreach (var opt in entity.Info.Options.Keys) {
                    var val = entity.Get(opt);
                    if (val != null)
                        entityElem.Attributes[opt] = val;
                }

                foreach (var node in entity.Nodes) {
                    Element n = new Element {
                        Attributes = new Dictionary<string, object> {
                            ["x"] = node.X - X * 8,
                            ["y"] = node.Y - Y * 8
                        }
                    };
                    entityElem.Children.Add(n);
                }

                entitiesElement.Children.Add(entityElem);
            }

            Element triggersElement = new Element {
                Attributes = new Dictionary<string, object>(),
                Name = "triggers",
                Children = new List<Element>()
            };
            ret.Children.Add(triggersElement);

            foreach (var tigger in Triggers) {
                Element triggersElem = new Element {
                    Name = tigger.Name,
                    Children = new List<Element>(),
                    Attributes = new Dictionary<string, object> {
                        ["x"] = tigger.X - X * 8,
                        ["y"] = tigger.Y - Y * 8,
                        ["width"] = tigger.Width,
                        ["height"] = tigger.Height,
                        ["originX"] = tigger.Origin.X,
                        ["originY"] = tigger.Origin.Y
                    }
                };

                foreach (var opt in tigger.Info.Options.Keys) {
                    var val = tigger.Get(opt);
                    if (val != null)
                        triggersElem.Attributes[opt] = val;
                }

                foreach (var node in tigger.Nodes) {
                    Element n = new Element {
                        Attributes = new Dictionary<string, object> {
                            ["x"] = node.X - X * 8,
                            ["y"] = node.Y - Y * 8
                        }
                    };
                    triggersElem.Children.Add(n);
                }

                triggersElement.Children.Add(triggersElem);
            }

            Element fgDecalsElem = new Element();
            fgDecalsElem.Name = "fgdecals";
            fgDecalsElem.Children = new List<Element>();
            ret.Children.Add(fgDecalsElem);
            foreach (var decal in FgDecals) {
                Element decalElem = new Element {
                    Attributes = new Dictionary<string, object> {
                        ["x"] = decal.Position.X,
                        ["y"] = decal.Position.Y,
                        ["scaleX"] = decal.Scale.X,
                        ["scaleY"] = decal.Scale.Y,
                        ["texture"] = decal.Texture
                    }
                };
                fgDecalsElem.Children.Add(decalElem);
            }

            Element bgDecalsElem = new Element();
            bgDecalsElem.Name = "bgdecals";
            bgDecalsElem.Children = new List<Element>();
            ret.Children.Add(bgDecalsElem);
            foreach (var decal in BgDecals) {
                Element decalElem = new Element {
                    Attributes = new Dictionary<string, object> {
                        ["x"] = decal.Position.X,
                        ["y"] = decal.Position.Y,
                        ["scaleX"] = decal.Scale.X,
                        ["scaleY"] = decal.Scale.Y,
                        ["texture"] = decal.Texture
                    }
                };
                bgDecalsElem.Children.Add(decalElem);
            }

            StringBuilder fgTiles = new StringBuilder();
            for (int y = 0; y < fgTileMap.Rows; y++) {
                for (int x = 0; x < fgTileMap.Columns; x++) {
                    fgTiles.Append(fgTileMap[x, y]);
                }

                fgTiles.Append("\n");
            }

            StringBuilder bgTiles = new StringBuilder();
            for (int y = 0; y < bgTileMap.Rows; y++) {
                for (int x = 0; x < bgTileMap.Columns; x++) {
                    bgTiles.Append(bgTileMap[x, y]);
                }

                bgTiles.Append("\n");
            }

            Element fgSolidsElem = new Element {
                Name = "solids",
                Attributes = new Dictionary<string, object> {
                    ["innerText"] = fgTiles.ToString()
                }
            };
            ret.Children.Add(fgSolidsElem);

            Element bgSolidsElem = new Element {
                Name = "bg",
                Attributes = new Dictionary<string, object> {
                    ["innerText"] = bgTiles.ToString()
                }
            };
            ret.Children.Add(bgSolidsElem);

            return ret;
        }

        public void AddEntity(Entity e) {
            AllEntities.Add(e);
            if (e is Plugin_Trigger)
                Triggers.Add(e);
            else
                Entities.Add(e);
            if (e.Tracked) {
                Type tracking = e.GetType();
                if (!TrackedEntities.ContainsKey(tracking))
                    TrackedEntities[tracking] = new List<Entity>();
                TrackedEntities[tracking].Add(e);
            }
        }

        public void RemoveEntity(Entity e) {
            AllEntities.Remove(e);
            Entities.Remove(e);
            Triggers.Remove(e);
            Type tracking = e.GetType();
            if (e.Tracked && TrackedEntities.ContainsKey(tracking)) {
                TrackedEntities[tracking].Remove(e);
                if (TrackedEntities[tracking].Count == 0)
                    TrackedEntities.Remove(tracking);
            }
        }

        public void MarkTrackedEntityDirty(Entity e) {
            if (e.Tracked) {
                DirtyTrackedEntities[e.GetType()] = true;
            }
        }
    }
}