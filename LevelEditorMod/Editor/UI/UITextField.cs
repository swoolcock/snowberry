﻿using Celeste.Mod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System;
using System.Linq;

namespace LevelEditorMod.Editor.UI {
    public class UITextField : UIElement {
        private bool selected, hovering;
        private int charIndex, selection;

        public string Value { get; private set; }
        private int[] widthAtIndex;
        private readonly Font font;

        private float lerp;

        public Color Line = Color.Teal;
        public Color LineSelected = Color.LimeGreen;
        public Color BG = Color.Black * 0.25f;
        public Color BGSelected = Color.Black * 0.5f;
        public Color FG = Calc.HexToColor("f0f0f0");

        private float timeOffset;

        private static string clipboard;

        protected char[] AllowedCharacters;

        public UITextField(Font font, int width, string input = "") {
            this.font = font;
            UpdateInput(input ?? "null");
            charIndex = selection = this.Value.Length;

            Width = Math.Max(1, width);
            Height = font.LineHeight;

            TextInput.OnInput += OnInput;
        }

        private void OnInput(char c) {
            if (Engine.Commands.Open || !selected)
                return;

            GetSelection(out int a, out int b);

            if (c == '\b' && Value.Length != 0 && !(a == 0 && b == 0)) {
                int nextCharIndex = a == b ? a - 1 : a;
                InsertString(nextCharIndex, b);
                selection = charIndex = nextCharIndex;
                timeOffset = Engine.Scene.TimeActive;
            } else if (!char.IsControl(c) && (AllowedCharacters == null || AllowedCharacters.Contains(c))) {
                UpdateInput(Value.Substring(0, a) + c + Value.Substring(b));
                selection = charIndex = a + 1;
                timeOffset = Engine.Scene.TimeActive;
            }
        }

        private void InsertString(int from, int to, string str = null)
            => UpdateInput(Value.Substring(0, from) + str + Value.Substring(to));

        private void UpdateInput(string str) {
            Value = str;
            widthAtIndex = new int[Value.Length + 1];
            int w = 0;
            for (int i = 0; i < widthAtIndex.Length - 1; i++) {
                widthAtIndex[i] = w;
                w += (int)font.Measure(Value[i]).X + 1;
            }
            widthAtIndex[widthAtIndex.Length - 1] = w;
            OnInputUpdate(Value);
        }

        protected virtual void OnInputUpdate(string input) { }

        private void GetSelection(out int a, out int b) {
            if (charIndex < selection) {
                a = charIndex; b = selection;
            } else if (selection < charIndex) {
                a = selection; b = charIndex;
            } else {
                a = b = charIndex;
            }
        }

        private static bool MustSeparate(char at, char previous)
            => char.GetUnicodeCategory(char.ToLower(at)) != char.GetUnicodeCategory(char.ToLower(previous));

        private int MoveIndex(int step, bool stepByWord) {
            int next = charIndex;

            if (stepByWord) {
                next += step;
                while (next > 0 && next < Value.Length && !MustSeparate(Value[next], Value[next - 1]))
                    next += step;
            } else
                next += step;

            return Calc.Clamp(next, 0, Value.Length); ;
        }

        public override void Update(Vector2 position = default) {
            base.Update(position);

            int mouseX = (int)Editor.Mouse.Screen.X;
            int mouseY = (int)Editor.Mouse.Screen.Y;
            bool inside = new Rectangle((int)position.X - 1, (int)position.Y - 1, Width + 2, Height + 2).Contains(mouseX, mouseY);

            if (MInput.Mouse.CheckLeftButton) {
                bool click = MInput.Mouse.PressedLeftButton;

                if (click)
                    selected = inside;

                if (selected) {
                    int i, d = mouseX - (int)position.X + 1;

                    for (i = 0; i < widthAtIndex.Length - 1; i++)
                        if (widthAtIndex[i + 1] >= d)
                            break;

                    if (i != charIndex) {
                        charIndex = i;
                        if (click)
                            selection = i;
                        timeOffset = Engine.Scene.TimeActive;
                    }
                }
            }

            if (selected) {
                bool shift = MInput.Keyboard.CurrentState[Keys.LeftShift] == KeyState.Down || MInput.Keyboard.CurrentState[Keys.RightShift] == KeyState.Down;
                bool ctrl = MInput.Keyboard.CurrentState[Keys.LeftControl] == KeyState.Down || MInput.Keyboard.CurrentState[Keys.RightControl] == KeyState.Down;
                
                if (MInput.Keyboard.Pressed(Keys.Escape)) {
                    selected = false;
                } else {
                    bool moved = false;
                    if (moved |= MInput.Keyboard.Pressed(Keys.Left))
                        charIndex = MoveIndex(-1, ctrl);
                    else if (moved |= MInput.Keyboard.Pressed(Keys.Right))
                        charIndex = MoveIndex(1, ctrl);
                    if (moved) {
                        timeOffset = Engine.Scene.TimeActive;
                        if (!shift)
                            selection = charIndex;
                    }
                }

                if (ctrl) {
                    bool copy = MInput.Keyboard.Pressed(Keys.C), cut = MInput.Keyboard.Pressed(Keys.X);

                    if (MInput.Keyboard.Pressed(Keys.A)) {
                        charIndex = Value.Length;
                        selection = 0;
                    }
                    
                    if (selection != charIndex && (copy || cut)) {
                        GetSelection(out int a, out int b);
                        clipboard = Value.Substring(a, b - a);
                        if (cut) {
                            InsertString(a, b);
                            selection = charIndex = a;
                        }
                    } else if (MInput.Keyboard.Pressed(Keys.V) && clipboard != null) {
                        GetSelection(out int a, out int b);
                        InsertString(a, b, clipboard);
                        selection = charIndex = a + clipboard.Length;
                        timeOffset = Engine.Scene.TimeActive;
                    }
                }
            }

            lerp = Calc.Approach(lerp, selected ? 1f : 0f, Engine.DeltaTime * 4f);
            hovering = inside;
        }

        public override void Render(Vector2 position = default) {
            base.Render(position);

            Draw.Rect(position, Width, Height, Color.Lerp(BG, BGSelected, hovering && !selected ? 0.25f : lerp));
            font.Draw(Value, position, Vector2.One, FG);

            Draw.Rect(position + Vector2.UnitY * Height, Width, 1, Line);
            if (lerp != 0f) {
                float ease = Ease.ExpoOut(lerp);
                Vector2 p = new Vector2(position.X + (1 - ease) * Width / 2f, position.Y + Height);
                Draw.Rect(p, (Width + 1) * ease, 1, Color.Lerp(Line, LineSelected, lerp));
            }

            if (selected) {
                if ((Engine.Scene.TimeActive - timeOffset) % 1f < 0.5f) {
                    Draw.Rect(position + Vector2.UnitX * widthAtIndex[charIndex], 1, font.LineHeight, FG);
                }
                if (selection != charIndex) {
                    int a = widthAtIndex[charIndex], b = widthAtIndex[selection];
                    if (a < b)
                        Draw.Rect(position + Vector2.UnitX * a, b - a, font.LineHeight, Color.Blue * 0.25f);
                    else
                        Draw.Rect(position + Vector2.UnitX * b, a - b, font.LineHeight, Color.Blue * 0.25f);
                }
            }
        }

        protected override void OnDestroy() {
            base.OnDestroy();

            TextInput.OnInput -= OnInput;
        }
    }
}