using PaintDotNet;
using PaintDotNet.Effects;
using PaintDotNet.IndirectUI;
using PaintDotNet.PropertySystem;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace BlendModesPlus
{
    public class PluginSupportInfo : IPluginSupportInfo
    {
        public string Author => base.GetType().Assembly.GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright;
        public string Copyright => base.GetType().Assembly.GetCustomAttribute<AssemblyDescriptionAttribute>().Description;
        public string DisplayName => base.GetType().Assembly.GetCustomAttribute<AssemblyProductAttribute>().Product;
        public Version Version => base.GetType().Assembly.GetName().Version;
        public Uri WebsiteUri => new Uri("https://forums.getpaint.net/index.php?showtopic=113214");
    }

    [PluginSupportInfo(typeof(PluginSupportInfo), DisplayName = "BlendModes Plus")]
    public class EffectPlugin : PropertyBasedEffect
    {
        private static readonly Bitmap StaticIcon = new Bitmap(typeof(EffectPlugin), "EffectPluginIcon.png");

        public EffectPlugin()
          : base("BlendModes Plus", StaticIcon, "Tools", EffectFlags.Configurable)
        {
        }

        private BlendModes BlendMode;
        private bool SwapLayers;
        private Surface SurfaceToBlend;
        private ImagePositions ImagePosition;
        private static readonly BinaryPixelOp normalOp = LayerBlendModeUtil.CreateCompositionOp(LayerBlendMode.Normal);

        private enum PropertyNames
        {
            ImageSource,
            ImageFile,
            ImagePosition,
            BlendMode,
            SwapLayers
        }

        private enum ImageSources
        {
            ClipBoard,
            File
        }

        private enum ImagePositions
        {
            Fill,
            Fit,
            Stretch,
            Center
        }

        private enum BlendModes
        {
            Normal,
            Additive,
            Average,
            Blue,
            Color,
            ColorBurn,
            ColorDodge,
            Cyan,
            Darken,
            Difference,
            Divide,
            Exclusion,
            Glow,
            GrainExtract,
            GrainMerge,
            Green,
            HardLight,
            HardMix,
            Hue,
            Lighten,
            LinearBurn,
            LinearDodge,
            LinearLight,
            Luminosity,
            Magenta,
            Multiply,
            Negation,
            Overlay,
            Phoenix,
            PinLight,
            Red,
            Reflect,
            Saturation,
            Screen,
            SignedDifference,
            SoftLight,
            Stamp,
            VividLight,
            Yellow
        }

        protected override PropertyCollection OnCreatePropertyCollection()
        {
            List<Property> props = new List<Property>
            {
                StaticListChoiceProperty.CreateForEnum(PropertyNames.ImageSource, ImageSources.File),
                new StringProperty(PropertyNames.ImageFile, string.Empty),
                StaticListChoiceProperty.CreateForEnum(PropertyNames.ImagePosition, ImagePositions.Stretch),
                StaticListChoiceProperty.CreateForEnum(PropertyNames.BlendMode, BlendModes.Normal),
                new BooleanProperty(PropertyNames.SwapLayers, false)
            };

            List<PropertyCollectionRule> propRules = new List<PropertyCollectionRule>
            {
                new ReadOnlyBoundToValueRule<object, StaticListChoiceProperty>(PropertyNames.ImageFile, PropertyNames.ImageSource, ImageSources.ClipBoard, false)
            };

            return new PropertyCollection(props, propRules);
        }

        protected override ControlInfo OnCreateConfigUI(PropertyCollection props)
        {
            ControlInfo configUI = CreateDefaultConfigUI(props);

            configUI.SetPropertyControlValue(PropertyNames.ImageSource, ControlInfoPropertyNames.DisplayName, "Image To Blend");
            configUI.SetPropertyControlType(PropertyNames.ImageSource, PropertyControlType.RadioButton);
            PropertyControlInfo Amount1Control = configUI.FindControlForPropertyName(PropertyNames.ImageSource);
            Amount1Control.SetValueDisplayName(ImageSources.ClipBoard, "Image From Clipboard");
            Amount1Control.SetValueDisplayName(ImageSources.File, "Image From File");

            configUI.SetPropertyControlValue(PropertyNames.ImageFile, ControlInfoPropertyNames.DisplayName, string.Empty);
            configUI.SetPropertyControlValue(PropertyNames.ImageFile, ControlInfoPropertyNames.FileTypes, new string[] { "bmp", "gif", "jpg", "jpeg", "png" });
            configUI.SetPropertyControlType(PropertyNames.ImageFile, PropertyControlType.FileChooser);

            configUI.SetPropertyControlValue(PropertyNames.ImagePosition, ControlInfoPropertyNames.DisplayName, "Image Position");
            PropertyControlInfo PositionControl = configUI.FindControlForPropertyName(PropertyNames.BlendMode);
            PositionControl.SetValueDisplayName(ImagePositions.Fill, "Fill");
            PositionControl.SetValueDisplayName(ImagePositions.Fit, "Fit");
            PositionControl.SetValueDisplayName(ImagePositions.Stretch, "Stretch");
            PositionControl.SetValueDisplayName(ImagePositions.Center, "Center");

            configUI.SetPropertyControlValue(PropertyNames.BlendMode, ControlInfoPropertyNames.DisplayName, "Blend Mode");
            PropertyControlInfo Amount3Control = configUI.FindControlForPropertyName(PropertyNames.BlendMode);
            Amount3Control.SetValueDisplayName(BlendModes.Normal, "Normal");
            Amount3Control.SetValueDisplayName(BlendModes.Additive, "Additive");
            Amount3Control.SetValueDisplayName(BlendModes.Average, "Average");
            Amount3Control.SetValueDisplayName(BlendModes.Blue, "Blue");
            Amount3Control.SetValueDisplayName(BlendModes.Color, "Color");
            Amount3Control.SetValueDisplayName(BlendModes.ColorBurn, "Color Burn");
            Amount3Control.SetValueDisplayName(BlendModes.ColorDodge, "Color Dodge");
            Amount3Control.SetValueDisplayName(BlendModes.Cyan, "Cyan");
            Amount3Control.SetValueDisplayName(BlendModes.Darken, "Darken");
            Amount3Control.SetValueDisplayName(BlendModes.Difference, "Difference");
            Amount3Control.SetValueDisplayName(BlendModes.Divide, "Divide");
            Amount3Control.SetValueDisplayName(BlendModes.Exclusion, "Exclusion");
            Amount3Control.SetValueDisplayName(BlendModes.Glow, "Glow");
            Amount3Control.SetValueDisplayName(BlendModes.GrainExtract, "Grain Extract");
            Amount3Control.SetValueDisplayName(BlendModes.GrainMerge, "Grain Merge");
            Amount3Control.SetValueDisplayName(BlendModes.Green, "Green");
            Amount3Control.SetValueDisplayName(BlendModes.HardLight, "Hard Light");
            Amount3Control.SetValueDisplayName(BlendModes.HardMix, "Hard Mix");
            Amount3Control.SetValueDisplayName(BlendModes.Hue, "Hue");
            Amount3Control.SetValueDisplayName(BlendModes.Lighten, "Lighten");
            Amount3Control.SetValueDisplayName(BlendModes.LinearBurn, "Linear Burn");
            Amount3Control.SetValueDisplayName(BlendModes.LinearDodge, "Linear Dodge");
            Amount3Control.SetValueDisplayName(BlendModes.LinearLight, "Linear Light");
            Amount3Control.SetValueDisplayName(BlendModes.Luminosity, "Luminosity");
            Amount3Control.SetValueDisplayName(BlendModes.Magenta, "Magenta");
            Amount3Control.SetValueDisplayName(BlendModes.Multiply, "Multiply");
            Amount3Control.SetValueDisplayName(BlendModes.Negation, "Negation");
            Amount3Control.SetValueDisplayName(BlendModes.Overlay, "Overlay");
            Amount3Control.SetValueDisplayName(BlendModes.Phoenix, "Phoenix");
            Amount3Control.SetValueDisplayName(BlendModes.PinLight, "Pin Light");
            Amount3Control.SetValueDisplayName(BlendModes.Red, "Red");
            Amount3Control.SetValueDisplayName(BlendModes.Reflect, "Reflect");
            Amount3Control.SetValueDisplayName(BlendModes.Saturation, "Saturation");
            Amount3Control.SetValueDisplayName(BlendModes.Screen, "Screen");
            Amount3Control.SetValueDisplayName(BlendModes.SignedDifference, "Signed Difference");
            Amount3Control.SetValueDisplayName(BlendModes.SoftLight, "Soft Light");
            Amount3Control.SetValueDisplayName(BlendModes.Stamp, "Stamp");
            Amount3Control.SetValueDisplayName(BlendModes.VividLight, "Vivid Light");
            Amount3Control.SetValueDisplayName(BlendModes.Yellow, "Yellow");

            configUI.SetPropertyControlValue(PropertyNames.SwapLayers, ControlInfoPropertyNames.DisplayName, string.Empty);
            configUI.SetPropertyControlValue(PropertyNames.SwapLayers, ControlInfoPropertyNames.Description, "Swap Layers");

            return configUI;
        }

        protected override void OnSetRenderInfo(PropertyBasedEffectConfigToken newToken, RenderArgs dstArgs, RenderArgs srcArgs)
        {
            ImageSources imageSource = (ImageSources)newToken.GetProperty<StaticListChoiceProperty>(PropertyNames.ImageSource).Value;
            string FilePath = newToken.GetProperty<StringProperty>(PropertyNames.ImageFile).Value;
            ImagePosition = (ImagePositions)newToken.GetProperty<StaticListChoiceProperty>(PropertyNames.ImagePosition).Value;
            BlendMode = (BlendModes)newToken.GetProperty<StaticListChoiceProperty>(PropertyNames.BlendMode).Value;
            SwapLayers = newToken.GetProperty<BooleanProperty>(PropertyNames.SwapLayers).Value;


            Rectangle selection = EnvironmentParameters.GetSelection(srcArgs.Surface.Bounds).GetBoundsInt();
            float selRatio = (float)selection.Width / selection.Height;

            Bitmap loadedImaged = null;
            if (imageSource == ImageSources.File)
            {
                if (File.Exists(FilePath))
                {
                    try
                    {
                        loadedImaged = new Bitmap(FilePath);
                    }
                    catch
                    {
                    }
                }
            }
            else
            {
                Thread t = new Thread(new ThreadStart(GetImageFromClipboard));
                t.SetApartmentState(ApartmentState.STA);
                t.Start();
                t.Join();

                void GetImageFromClipboard()
                {
                    try
                    {
                        IDataObject clippy = Clipboard.GetDataObject();
                        if (clippy == null)
                            return;

                        if (Clipboard.ContainsData("PNG"))
                        {
                            Object pngObject = Clipboard.GetData("PNG");
                            if (pngObject is MemoryStream pngStream)
                                loadedImaged = (Bitmap)Image.FromStream(pngStream);
                        }
                        else if (clippy.GetDataPresent(DataFormats.Bitmap))
                        {
                            loadedImaged = (Bitmap)clippy.GetData(typeof(Bitmap));
                        }
                    }
                    catch
                    {
                    }
                }
            }

            if (loadedImaged != null)
            {
                if (this.SurfaceToBlend == null)
                {
                    this.SurfaceToBlend = new Surface(srcArgs.Size);
                }
                else
                {
                    this.SurfaceToBlend.Clear(ColorBgra.Transparent);
                }

                Surface imageSurface = Surface.CopyFromBitmap(loadedImaged);

                switch (ImagePosition)
                {
                    case ImagePositions.Fill:
                        Size ratioSize = new Size(imageSurface.Width, imageSurface.Height);
                        if (ratioSize.Width < ratioSize.Height * selRatio)
                            ratioSize.Height = (int)Math.Round(imageSurface.Width / selRatio);
                        else if (ratioSize.Width > ratioSize.Height * selRatio)
                            ratioSize.Width = (int)Math.Round(imageSurface.Height * selRatio);

                        Rectangle srcRect = new Rectangle
                        {
                            X = (int)Math.Round(Math.Abs(ratioSize.Width - imageSurface.Width) / 2f),
                            Y = (int)Math.Round(Math.Abs(ratioSize.Height - imageSurface.Height) / 2f),
                            Width = ratioSize.Width,
                            Height = ratioSize.Height
                        };

                        using (Surface ratioSurface = new Surface(ratioSize))
                        {
                            ratioSurface.CopySurface(imageSurface, Point.Empty, srcRect);
                            SurfaceToBlend.FitSurface(ResamplingAlgorithm.Bicubic, ratioSurface, selection);
                        }
                        break;

                    case ImagePositions.Fit:
                        float imageRatio = (float)imageSurface.Width / imageSurface.Height;
                        Size newSize = imageSurface.Size;

                        if (imageSurface.Width < imageSurface.Height * selRatio)
                        {
                            int selMin = Math.Min(selection.Width, selection.Height);

                            newSize.Width = (int)Math.Round(selMin * imageRatio);
                            newSize.Height = selMin;
                        }
                        else if (imageSurface.Width > imageSurface.Height * selRatio)
                        {
                            int selMax = Math.Max(selection.Width, selection.Height);

                            newSize.Width = selMax;
                            newSize.Height = (int)Math.Round(selMax / imageRatio);
                        }

                        using (Surface ratioSurface = new Surface(newSize))
                        {
                            ratioSurface.FitSurface(ResamplingAlgorithm.Bicubic, imageSurface);
                            Point dstOffset = new Point
                            {
                                X = selection.Left + ((newSize.Width < selection.Width) ? (int)Math.Round((selection.Width - newSize.Width) / 2f) : 0),
                                Y = selection.Top + ((newSize.Height < selection.Height) ? (int)Math.Round((selection.Height - newSize.Height) / 2f) : 0),
                            };
                            SurfaceToBlend.CopySurface(ratioSurface, dstOffset);
                        }
                        break;

                    case ImagePositions.Stretch:
                        SurfaceToBlend.FitSurface(ResamplingAlgorithm.Bicubic, imageSurface, selection);
                        break;

                    case ImagePositions.Center:
                        Point destOffset = new Point
                        {
                            X = selection.Left + (selection.Width - imageSurface.Width) / 2,
                            Y = selection.Top + (selection.Height - imageSurface.Height) / 2
                        };
                        SurfaceToBlend.CopySurface(imageSurface, destOffset);

                        break;
                }

                imageSurface.Dispose();
                loadedImaged.Dispose();
            }
            else
            {
                this.SurfaceToBlend?.Dispose();
                this.SurfaceToBlend = null;
            }


            base.OnSetRenderInfo(newToken, dstArgs, srcArgs);
        }

        protected override void OnRender(Rectangle[] renderRects, int startIndex, int length)
        {
            if (length == 0) return;
            for (int i = startIndex; i < startIndex + length; ++i)
            {
                Render(DstArgs.Surface, SrcArgs.Surface, renderRects[i]);
            }
        }

        private void Render(Surface dst, Surface src, Rectangle rect)
        {
            if (this.SurfaceToBlend == null)
            {
                dst.CopySurface(src, rect.Location, rect);
                return;
            }

            ColorBgra colorBgra1;
            ColorBgra colorBgra2;

            for (int y = rect.Top; y < rect.Bottom; y++)
            {
                if (IsCancelRequested) return;
                for (int x = rect.Left; x < rect.Right; x++)
                {
                    if (SwapLayers)
                    {
                        colorBgra1 = this.SurfaceToBlend[x, y];
                        colorBgra2 = src[x, y];
                    }
                    else
                    {
                        colorBgra1 = src[x, y];
                        colorBgra2 = this.SurfaceToBlend[x, y];
                    }

                    dst[x, y] = BlendedPixel(colorBgra1, colorBgra2, this.BlendMode);
                }
            }
        }

        private static ColorBgra BlendedPixel(ColorBgra lhs, ColorBgra rhs, BlendModes blendMode)
        {
            switch (blendMode)
            {
                case BlendModes.Normal:
                    return normalOp.Apply(lhs, rhs);

                case BlendModes.Additive:
                    byte r1 = Int32Util.ClampToByte(lhs.R + rhs.R);
                    byte g1 = Int32Util.ClampToByte(lhs.G + rhs.G);
                    byte b1 = Int32Util.ClampToByte(lhs.B + rhs.B);
                    byte a1 = normalOp.Apply(lhs, rhs).A;
                    return ColorBgra.FromBgra(b1, g1, r1, a1);

                case BlendModes.Average:
                    byte r2 = Int32Util.ClampToByte((lhs.R + rhs.R) / 2);
                    byte g2 = Int32Util.ClampToByte((lhs.G + rhs.G) / 2);
                    byte b2 = Int32Util.ClampToByte((lhs.B + rhs.B) / 2);
                    byte a2 = normalOp.Apply(lhs, rhs).A;
                    return ColorBgra.FromBgra(b2, g2, r2, a2);

                case BlendModes.Blue:
                    byte a3 = normalOp.Apply(lhs, rhs).A;
                    return ColorBgra.FromBgra(rhs.B, lhs.G, lhs.R, a3);

                //case BlendModes.Clone:
                //    byte r3 = 0;
                //    byte g3 = 0;
                //    byte b3 = 0;
                //    byte a4 = normalOp.Apply(lhs, rhs).A;
                //    return ColorBgra.FromBgra(b3, g3, r3, a4);

                case BlendModes.Color:
                    HsvColor hsvColor1 = HsvColor.FromColor(lhs);
                    HsvColor hsvColor2 = HsvColor.FromColor(rhs);
                    Color color1 = new HsvColor()
                    {
                        Hue = hsvColor2.Hue,
                        Saturation = hsvColor2.Saturation,
                        Value = hsvColor1.Value
                    }.ToColor();
                    byte a5 = normalOp.Apply(lhs, rhs).A;
                    return ColorBgra.FromBgra(color1.B, color1.G, color1.R, a5);

                case BlendModes.ColorBurn:
                    byte r4 = rhs.R == (byte)0 ? (byte)0 : Int32Util.ClampToByte(byte.MaxValue - (byte.MaxValue - lhs.R << 8) / rhs.R);
                    byte g4 = rhs.G == (byte)0 ? (byte)0 : Int32Util.ClampToByte(byte.MaxValue - (byte.MaxValue - lhs.G << 8) / rhs.G);
                    byte b4 = rhs.B == (byte)0 ? (byte)0 : Int32Util.ClampToByte(byte.MaxValue - (byte.MaxValue - lhs.B << 8) / rhs.B);
                    byte a6 = normalOp.Apply(lhs, rhs).A;
                    return ColorBgra.FromBgra(b4, g4, r4, a6);

                case BlendModes.ColorDodge:
                    byte r5 = rhs.R == byte.MaxValue ? byte.MaxValue : Int32Util.ClampToByte((lhs.R << 8) / (byte.MaxValue - rhs.R));
                    byte g5 = rhs.G == byte.MaxValue ? byte.MaxValue : Int32Util.ClampToByte((lhs.G << 8) / (byte.MaxValue - rhs.G));
                    byte b5 = rhs.B == byte.MaxValue ? byte.MaxValue : Int32Util.ClampToByte((lhs.B << 8) / (byte.MaxValue - rhs.B));
                    byte a7 = normalOp.Apply(lhs, rhs).A;
                    return ColorBgra.FromBgra(b5, g5, r5, a7);

                case BlendModes.Cyan:
                    byte a8 = normalOp.Apply(lhs, rhs).A;
                    return ColorBgra.FromBgra(rhs.B, rhs.G, lhs.R, a8);

                case BlendModes.Darken:
                    byte r6 = Int32Util.ClampToByte((int)Math.Min(lhs.R, rhs.R));
                    byte g6 = Int32Util.ClampToByte((int)Math.Min(lhs.G, rhs.G));
                    byte b6 = Int32Util.ClampToByte((int)Math.Min(lhs.B, rhs.B));
                    byte a9 = normalOp.Apply(lhs, rhs).A;
                    return ColorBgra.FromBgra(b6, g6, r6, a9);

                case BlendModes.Difference:
                    byte r7 = Int32Util.ClampToByte(Math.Abs(lhs.R - rhs.R));
                    byte g7 = Int32Util.ClampToByte(Math.Abs(lhs.G - rhs.G));
                    byte b7 = Int32Util.ClampToByte(Math.Abs(lhs.B - rhs.B));
                    byte a10 = normalOp.Apply(lhs, rhs).A;
                    return ColorBgra.FromBgra(b7, g7, r7, a10);

                case BlendModes.Divide:
                    byte r8 = Int32Util.ClampToByte(lhs.R * byte.MaxValue / (rhs.R + 1));
                    byte g8 = Int32Util.ClampToByte(lhs.G * byte.MaxValue / (rhs.G + 1));
                    byte b8 = Int32Util.ClampToByte(lhs.B * byte.MaxValue / (rhs.B + 1));
                    byte a11 = normalOp.Apply(lhs, rhs).A;
                    return ColorBgra.FromBgra(b8, g8, r8, a11);

                case BlendModes.Exclusion:
                    byte r9 = Int32Util.ClampToByte(lhs.R + rhs.R - 2 * lhs.R * rhs.R / byte.MaxValue);
                    byte g9 = Int32Util.ClampToByte(lhs.G + rhs.G - 2 * lhs.G * rhs.G / byte.MaxValue);
                    byte b9 = Int32Util.ClampToByte(lhs.B + rhs.B - 2 * lhs.B * rhs.B / byte.MaxValue);
                    byte a12 = normalOp.Apply(lhs, rhs).A;
                    return ColorBgra.FromBgra(b9, g9, r9, a12);

                //case BlendModes.Freeze:
                //    byte r10 = rhs.R != (byte)0 ? DoubleUtil.ClampToByte(((double)byte.MaxValue - Math.Pow((double)(byte.MaxValue - lhs.R), 2.0)) / (double)rhs.R) : (byte)0;
                //    byte g10 = rhs.G != (byte)0 ? DoubleUtil.ClampToByte(((double)byte.MaxValue - Math.Pow((double)(byte.MaxValue - lhs.G), 2.0)) / (double)rhs.G) : (byte)0;
                //    byte b10 = rhs.B != (byte)0 ? DoubleUtil.ClampToByte(((double)byte.MaxValue - Math.Pow((double)(byte.MaxValue - lhs.B), 2.0)) / (double)rhs.B) : (byte)0;
                //    byte a13 = normalOp.Apply(lhs, rhs).A;
                //    return ColorBgra.FromBgra(b10, g10, r10, a13);

                case BlendModes.Glow:
                    byte r11 = Int32Util.ClampToByte(lhs.R == byte.MaxValue ? byte.MaxValue : rhs.R * rhs.R / (byte.MaxValue - lhs.R));
                    byte g11 = Int32Util.ClampToByte(lhs.G == byte.MaxValue ? byte.MaxValue : rhs.G * rhs.G / (byte.MaxValue - lhs.G));
                    byte b11 = Int32Util.ClampToByte(lhs.B == byte.MaxValue ? byte.MaxValue : rhs.B * rhs.B / (byte.MaxValue - lhs.B));
                    byte a14 = normalOp.Apply(lhs, rhs).A;
                    return ColorBgra.FromBgra(b11, g11, r11, a14);

                case BlendModes.Green:
                    byte a15 = normalOp.Apply(lhs, rhs).A;
                    return ColorBgra.FromBgra(lhs.B, rhs.G, lhs.R, a15);

                case BlendModes.GrainExtract:
                    byte r12 = Int32Util.ClampToByte(lhs.R - rhs.R + 128);
                    byte g12 = Int32Util.ClampToByte(lhs.G - rhs.G + 128);
                    byte b12 = Int32Util.ClampToByte(lhs.B - rhs.B + 128);
                    byte a16 = normalOp.Apply(lhs, rhs).A;
                    return ColorBgra.FromBgra(b12, g12, r12, a16);

                case BlendModes.GrainMerge:
                    byte r13 = Int32Util.ClampToByte(lhs.R + rhs.R - 128);
                    byte g13 = Int32Util.ClampToByte(lhs.G + rhs.G - 128);
                    byte b13 = Int32Util.ClampToByte(lhs.B + rhs.B - 128);
                    byte a17 = normalOp.Apply(lhs, rhs).A;
                    return ColorBgra.FromBgra(b13, g13, r13, a17);

                case BlendModes.HardLight:
                    byte a18 = normalOp.Apply(lhs, rhs).A;
                    byte r14 = rhs.R > (byte)128 ? Int32Util.ClampToByte(byte.MaxValue - (byte.MaxValue - lhs.R) * (byte.MaxValue - 2 * (rhs.R - 128)) / byte.MaxValue) : Int32Util.ClampToByte(2 * lhs.R * rhs.R / byte.MaxValue);
                    byte g14 = rhs.G > (byte)128 ? Int32Util.ClampToByte(byte.MaxValue - (byte.MaxValue - lhs.G) * (byte.MaxValue - 2 * (rhs.G - 128)) / byte.MaxValue) : Int32Util.ClampToByte(2 * lhs.G * rhs.G / byte.MaxValue);
                    byte b14 = rhs.B > (byte)128 ? Int32Util.ClampToByte(byte.MaxValue - (byte.MaxValue - lhs.B) * (byte.MaxValue - 2 * (rhs.G - 128)) / byte.MaxValue) : Int32Util.ClampToByte(2 * lhs.B * rhs.B / byte.MaxValue);
                    return ColorBgra.FromBgra(b14, g14, r14, a18);

                case BlendModes.HardMix:
                    byte a19 = normalOp.Apply(lhs, rhs).A;
                    byte r15 = rhs.R >= byte.MaxValue - lhs.R ? byte.MaxValue : (byte)0;
                    byte g15 = rhs.G >= byte.MaxValue - lhs.G ? byte.MaxValue : (byte)0;
                    byte b15 = rhs.B >= byte.MaxValue - lhs.B ? byte.MaxValue : (byte)0;
                    return ColorBgra.FromBgra(b15, g15, r15, a19);

                //case BlendModes.Heat:
                //    byte r16 = lhs.R != (byte)0 ? DoubleUtil.ClampToByte(((double)byte.MaxValue - Math.Pow((double)(byte.MaxValue - rhs.R), 2.0)) / (double)lhs.R) : (byte)0;
                //    byte g16 = lhs.G != (byte)0 ? DoubleUtil.ClampToByte(((double)byte.MaxValue - Math.Pow((double)(byte.MaxValue - rhs.G), 2.0)) / (double)lhs.G) : (byte)0;
                //    byte b16 = lhs.B != (byte)0 ? DoubleUtil.ClampToByte(((double)byte.MaxValue - Math.Pow((double)(byte.MaxValue - rhs.B), 2.0)) / (double)lhs.B) : (byte)0;
                //    byte a20 = normalOp.Apply(lhs, rhs).A;
                //    return ColorBgra.FromBgra(b16, g16, r16, a20);

                case BlendModes.Hue:
                    HsvColor hsvColor3 = HsvColor.FromColor(lhs);
                    HsvColor hsvColor4 = HsvColor.FromColor(rhs);
                    Color color2 = new HsvColor()
                    {
                        Hue = hsvColor4.Hue,
                        Saturation = hsvColor3.Saturation,
                        Value = hsvColor3.Value
                    }.ToColor();
                    byte a21 = normalOp.Apply(lhs, rhs).A;
                    return ColorBgra.FromBgra(color2.B, color2.G, color2.R, a21);

                //case BlendModes.Interpolation:
                //    byte r17 = DoubleUtil.ClampToByte(128.0 - 64.0 * Math.Cos(Math.PI * (double)lhs.R) - 64.0 * Math.Cos(Math.PI * (double)rhs.R));
                //    byte g17 = DoubleUtil.ClampToByte(128.0 - 64.0 * Math.Cos(Math.PI * (double)lhs.G) - 64.0 * Math.Cos(Math.PI * (double)rhs.G));
                //    byte b17 = DoubleUtil.ClampToByte(128.0 - 64.0 * Math.Cos(Math.PI * (double)lhs.B) - 64.0 * Math.Cos(Math.PI * (double)rhs.B));
                //    byte a22 = normalOp.Apply(lhs, rhs).A;
                //    return ColorBgra.FromBgra(b17, g17, r17, a22);

                case BlendModes.Lighten:
                    byte r18 = Int32Util.ClampToByte((int)Math.Max(lhs.R, rhs.R));
                    byte g18 = Int32Util.ClampToByte((int)Math.Max(lhs.G, rhs.G));
                    byte b18 = Int32Util.ClampToByte((int)Math.Max(lhs.B, rhs.B));
                    byte a23 = normalOp.Apply(lhs, rhs).A;
                    return ColorBgra.FromBgra(b18, g18, r18, a23);

                case BlendModes.LinearBurn:
                    byte r19 = Int32Util.ClampToByte(lhs.R + rhs.R - byte.MaxValue);
                    byte g19 = Int32Util.ClampToByte(lhs.G + rhs.G - byte.MaxValue);
                    byte b19 = Int32Util.ClampToByte(lhs.B + rhs.B - byte.MaxValue);
                    byte a24 = normalOp.Apply(lhs, rhs).A;
                    return ColorBgra.FromBgra(b19, g19, r19, a24);

                case BlendModes.LinearDodge:
                    byte r20 = Int32Util.ClampToByte(lhs.R + rhs.R);
                    byte g20 = Int32Util.ClampToByte(lhs.G + rhs.G);
                    byte b20 = Int32Util.ClampToByte(lhs.B + rhs.B);
                    byte a25 = normalOp.Apply(lhs, rhs).A;
                    return ColorBgra.FromBgra(b20, g20, r20, a25);

                case BlendModes.LinearLight:
                    byte r21 = Int32Util.ClampToByte(lhs.R + 2 * rhs.R - byte.MaxValue);
                    byte g21 = Int32Util.ClampToByte(lhs.G + 2 * rhs.G - byte.MaxValue);
                    byte b21 = Int32Util.ClampToByte(lhs.B + 2 * rhs.B - byte.MaxValue);
                    byte a26 = normalOp.Apply(lhs, rhs).A;
                    return ColorBgra.FromBgra(b21, g21, r21, a26);

                case BlendModes.Luminosity:
                    HsvColor hsvColor5 = HsvColor.FromColor(lhs);
                    HsvColor hsvColor6 = HsvColor.FromColor(rhs);
                    Color color3 = new HsvColor()
                    {
                        Hue = hsvColor5.Hue,
                        Saturation = hsvColor5.Saturation,
                        Value = hsvColor6.Value
                    }.ToColor();
                    byte a27 = normalOp.Apply(lhs, rhs).A;
                    return ColorBgra.FromBgra(color3.B, color3.G, color3.R, a27);

                case BlendModes.Magenta:
                    byte a28 = normalOp.Apply(lhs, rhs).A;
                    return ColorBgra.FromBgra(rhs.B, lhs.G, rhs.R, a28);

                case BlendModes.Multiply:
                    byte r22 = Int32Util.ClampToByte(lhs.R * rhs.R / byte.MaxValue);
                    byte g22 = Int32Util.ClampToByte(lhs.G * rhs.G / byte.MaxValue);
                    byte b22 = Int32Util.ClampToByte(lhs.B * rhs.B / byte.MaxValue);
                    byte a29 = normalOp.Apply(lhs, rhs).A;
                    return ColorBgra.FromBgra(b22, g22, r22, a29);

                case BlendModes.Negation:
                    byte r23 = Int32Util.ClampToByte(byte.MaxValue - Math.Abs(byte.MaxValue - lhs.R - rhs.R));
                    byte g23 = Int32Util.ClampToByte(byte.MaxValue - Math.Abs(byte.MaxValue - lhs.G - rhs.G));
                    byte b23 = Int32Util.ClampToByte(byte.MaxValue - Math.Abs(byte.MaxValue - lhs.B - rhs.B));
                    byte a30 = normalOp.Apply(lhs, rhs).A;
                    return ColorBgra.FromBgra(b23, g23, r23, a30);

                case BlendModes.Overlay:
                    byte r24 = lhs.R < (byte)128 ? Int32Util.ClampToByte(2 * rhs.R * lhs.R / byte.MaxValue) : Int32Util.ClampToByte(byte.MaxValue - 2 * (byte.MaxValue - rhs.R) * (byte.MaxValue - lhs.R) / byte.MaxValue);
                    byte g24 = lhs.G < (byte)128 ? Int32Util.ClampToByte(2 * rhs.G * lhs.G / byte.MaxValue) : Int32Util.ClampToByte(byte.MaxValue - 2 * (byte.MaxValue - rhs.G) * (byte.MaxValue - lhs.G) / byte.MaxValue);
                    byte b24 = lhs.B < (byte)128 ? Int32Util.ClampToByte(2 * rhs.B * lhs.B / byte.MaxValue) : Int32Util.ClampToByte(byte.MaxValue - 2 * (byte.MaxValue - rhs.B) * (byte.MaxValue - lhs.B) / byte.MaxValue);
                    byte a31 = normalOp.Apply(lhs, rhs).A;
                    return ColorBgra.FromBgra(b24, g24, r24, a31);

                case BlendModes.Phoenix:
                    byte r25 = Int32Util.ClampToByte((int)Math.Min(lhs.R, rhs.R) - (int)Math.Max(lhs.R, rhs.R) + byte.MaxValue);
                    byte g25 = Int32Util.ClampToByte((int)Math.Min(lhs.G, rhs.G) - (int)Math.Max(lhs.G, rhs.G) + byte.MaxValue);
                    byte b25 = Int32Util.ClampToByte((int)Math.Min(lhs.B, rhs.B) - (int)Math.Max(lhs.B, rhs.B) + byte.MaxValue);
                    byte a32 = normalOp.Apply(lhs, rhs).A;
                    return ColorBgra.FromBgra(b25, g25, r25, a32);

                case BlendModes.PinLight:
                    byte r26 = 0;
                    byte g26 = 0;
                    byte b26 = 0;
                    if (lhs.R < 2 * rhs.R - byte.MaxValue)
                        r26 = Int32Util.ClampToByte(2 * rhs.R - byte.MaxValue);
                    if (lhs.G < 2 * rhs.G - byte.MaxValue)
                        g26 = Int32Util.ClampToByte(2 * rhs.G - byte.MaxValue);
                    if (lhs.B < 2 * rhs.B - byte.MaxValue)
                        b26 = Int32Util.ClampToByte(2 * rhs.B - byte.MaxValue);
                    if (lhs.R > 2 * rhs.R - byte.MaxValue && lhs.R < 2 * rhs.R)
                        r26 = lhs.R;
                    if (lhs.G > 2 * rhs.G - byte.MaxValue && lhs.G < 2 * rhs.G)
                        g26 = lhs.G;
                    if (lhs.B > 2 * rhs.B - byte.MaxValue && lhs.B < 2 * rhs.B)
                        b26 = lhs.B;
                    if (lhs.R > 2 * rhs.R)
                        r26 = Int32Util.ClampToByte(2 * rhs.R);
                    if (lhs.G > 2 * rhs.G)
                        g26 = Int32Util.ClampToByte(2 * rhs.G);
                    if (lhs.B > 2 * rhs.B)
                        b26 = Int32Util.ClampToByte(2 * rhs.B);
                    byte a33 = normalOp.Apply(lhs, rhs).A;
                    return ColorBgra.FromBgra(b26, g26, r26, a33);

                case BlendModes.Red:
                    byte a34 = normalOp.Apply(lhs, rhs).A;
                    return ColorBgra.FromBgra(lhs.B, lhs.G, rhs.R, a34);

                case BlendModes.Reflect:
                    byte r27 = rhs.R == byte.MaxValue ? byte.MaxValue : Int32Util.ClampToByte(lhs.R * lhs.R / (byte.MaxValue - rhs.R));
                    byte g27 = rhs.G == byte.MaxValue ? byte.MaxValue : Int32Util.ClampToByte(lhs.G * lhs.G / (byte.MaxValue - rhs.G));
                    byte b27 = rhs.B == byte.MaxValue ? byte.MaxValue : Int32Util.ClampToByte(lhs.B * lhs.B / (byte.MaxValue - rhs.B));
                    byte a35 = normalOp.Apply(lhs, rhs).A;
                    return ColorBgra.FromBgra(b27, g27, r27, a35);

                case BlendModes.Saturation:
                    HsvColor hsvColor7 = HsvColor.FromColor(lhs);
                    HsvColor hsvColor8 = HsvColor.FromColor(rhs);
                    Color color4 = new HsvColor()
                    {
                        Hue = hsvColor7.Hue,
                        Saturation = hsvColor8.Saturation,
                        Value = hsvColor7.Value
                    }.ToColor();
                    byte a36 = normalOp.Apply(lhs, rhs).A;
                    return ColorBgra.FromBgra(color4.B, color4.G, color4.R, a36);

                case BlendModes.Screen:
                    byte r28 = Int32Util.ClampToByte(byte.MaxValue - ((byte.MaxValue - lhs.R) * (byte.MaxValue - rhs.R) >> 8));
                    byte g28 = Int32Util.ClampToByte(byte.MaxValue - ((byte.MaxValue - lhs.G) * (byte.MaxValue - rhs.G) >> 8));
                    byte b28 = Int32Util.ClampToByte(byte.MaxValue - ((byte.MaxValue - lhs.B) * (byte.MaxValue - rhs.B) >> 8));
                    byte a37 = normalOp.Apply(lhs, rhs).A;
                    return ColorBgra.FromBgra(b28, g28, r28, a37);
                    
                case BlendModes.SignedDifference:
                    byte r29 = Int32Util.ClampToByte((lhs.R - rhs.R) / 2 + 128);
                    byte g29 = Int32Util.ClampToByte((lhs.G - rhs.G) / 2 + 128);
                    byte b29 = Int32Util.ClampToByte((lhs.B - rhs.B) / 2 + 128);
                    byte a38 = normalOp.Apply(lhs, rhs).A;
                    return ColorBgra.FromBgra(b29, g29, r29, a38);

                //case BlendModes.SoftBurn:
                //    byte a39 = normalOp.Apply(lhs, rhs).A;
                //    byte r30 = lhs.R + rhs.R >= byte.MaxValue ? (rhs.R != (byte)0 ? Int32Util.ClampToByte((1 - 128 * (byte.MaxValue - lhs.R)) / rhs.R) : (byte)0) : (lhs.R != byte.MaxValue ? Int32Util.ClampToByte(128 * rhs.R / (byte.MaxValue - lhs.R)) : (byte)0);
                //    byte g30 = lhs.G + rhs.G >= byte.MaxValue ? (rhs.G != (byte)0 ? Int32Util.ClampToByte((1 - 128 * (byte.MaxValue - lhs.G)) / rhs.G) : (byte)0) : (lhs.G != byte.MaxValue ? Int32Util.ClampToByte(128 * rhs.G / (byte.MaxValue - lhs.G)) : (byte)0);
                //    byte b30 = lhs.B + rhs.B >= byte.MaxValue ? (rhs.B != (byte)0 ? Int32Util.ClampToByte((1 - 128 * (byte.MaxValue - lhs.B)) / rhs.B) : (byte)0) : (lhs.B != byte.MaxValue ? Int32Util.ClampToByte(128 * rhs.B / (byte.MaxValue - lhs.B)) : (byte)0);
                //    return ColorBgra.FromBgra(b30, g30, r30, a39);

                //case BlendModes.SoftDodge:
                //    byte a40 = normalOp.Apply(lhs, rhs).A;
                //    byte r31 = lhs.R + rhs.R >= byte.MaxValue ? (lhs.R != (byte)0 ? Int32Util.ClampToByte((byte.MaxValue - 128 * (byte.MaxValue - rhs.R)) / lhs.R) : (byte)0) : (rhs.R != byte.MaxValue ? Int32Util.ClampToByte(128 * lhs.R / (byte.MaxValue - rhs.R)) : byte.MaxValue);
                //    byte g31 = lhs.G + rhs.G >= byte.MaxValue ? (lhs.G != (byte)0 ? Int32Util.ClampToByte((byte.MaxValue - 128 * (byte.MaxValue - rhs.G)) / lhs.G) : (byte)0) : (rhs.G != byte.MaxValue ? Int32Util.ClampToByte(128 * lhs.G / (byte.MaxValue - rhs.G)) : byte.MaxValue);
                //    byte b31 = lhs.B + rhs.B >= byte.MaxValue ? (lhs.B != (byte)0 ? Int32Util.ClampToByte((byte.MaxValue - 128 * (byte.MaxValue - rhs.B)) / lhs.B) : (byte)0) : (rhs.B != byte.MaxValue ? Int32Util.ClampToByte(128 * lhs.B / (byte.MaxValue - rhs.B)) : byte.MaxValue);
                //    return ColorBgra.FromBgra(b31, g31, r31, a40);

                case BlendModes.SoftLight:
                    byte r32 = DoubleUtil.ClampToByte(rhs.R < 128 ? (float)(2 * ((lhs.R >> 1) + 64)) * ((float)rhs.R / (float)byte.MaxValue) : (float)(byte.MaxValue - (double)(2 * (byte.MaxValue - ((lhs.R >> 1) + 64))) * (double)(byte.MaxValue - rhs.R) / byte.MaxValue));
                    byte g32 = DoubleUtil.ClampToByte(rhs.G < 128 ? (float)(2 * ((lhs.G >> 1) + 64)) * ((float)rhs.G / (float)byte.MaxValue) : (float)(byte.MaxValue - (double)(2 * (byte.MaxValue - ((lhs.G >> 1) + 64))) * (double)(byte.MaxValue - rhs.G) / byte.MaxValue));
                    byte b32 = DoubleUtil.ClampToByte(rhs.B < 128 ? (float)(2 * ((lhs.B >> 1) + 64)) * ((float)rhs.B / (float)byte.MaxValue) : (float)(byte.MaxValue - (double)(2 * (byte.MaxValue - ((lhs.B >> 1) + 64))) * (double)(byte.MaxValue - rhs.B) / byte.MaxValue));
                    byte a41 = normalOp.Apply(lhs, rhs).A;
                    return ColorBgra.FromBgra(b32, g32, r32, a41);

                case BlendModes.Stamp:
                    byte r33 = Int32Util.ClampToByte(lhs.R + 2 * rhs.R - byte.MaxValue);
                    byte g33 = Int32Util.ClampToByte(lhs.G + 2 * rhs.G - byte.MaxValue);
                    byte b33 = Int32Util.ClampToByte(lhs.B + 2 * rhs.B - byte.MaxValue);
                    byte a42 = normalOp.Apply(lhs, rhs).A;
                    return ColorBgra.FromBgra(b33, g33, r33, a42);

                case BlendModes.VividLight:
                    byte a43 = normalOp.Apply(lhs, rhs).A;
                    byte r34 = rhs.R <= (byte)128 ? Int32Util.ClampToByte(lhs.R + 2 * rhs.R - byte.MaxValue) : Int32Util.ClampToByte(lhs.R + 2 * (rhs.R - 128));
                    byte g34 = rhs.G <= (byte)128 ? Int32Util.ClampToByte(lhs.G + 2 * rhs.G - byte.MaxValue) : Int32Util.ClampToByte(lhs.G + 2 * (rhs.G - 128));
                    byte b34 = rhs.B <= (byte)128 ? Int32Util.ClampToByte(lhs.B + 2 * rhs.B - byte.MaxValue) : Int32Util.ClampToByte(lhs.B + 2 * (rhs.B - 128));
                    return ColorBgra.FromBgra(b34, g34, r34, a43);

                case BlendModes.Yellow:
                    byte a44 = normalOp.Apply(lhs, rhs).A;
                    return ColorBgra.FromBgra(lhs.B, rhs.G, rhs.R, a44);

                default:
                    return normalOp.Apply(lhs, rhs);
            }
        }
    }
}
