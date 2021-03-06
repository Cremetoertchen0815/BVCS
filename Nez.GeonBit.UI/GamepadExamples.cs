using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nez.ExtendedContent.DataTypes;
using Nez.GeonBit.UI.Entities;
using Nez.GeonBit.UI.Entities.TextValidators;
using System.Collections.Generic;
using UIEntity = Nez.GeonBit.UI.Entities.Entity;


namespace Nez.GeonBit.UI.Example
{
    /// <summary>
    /// GamePadExamples
    /// </summary>
    public class GamePadExamples : Game
    {
        // graphics and spritebatch
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        // all the example panels (screens)
        private List<Panel> panels = new List<Panel>();

        // buttons to rotate examples
        private Button nextExampleButton;

        // paragraph that shows the currently active entity
        private Paragraph targetEntityShow;

        // current example shown
        private int currExample = 0;

        /// <summary>
        /// Class Constructor
        /// </summary>
        public GamePadExamples()
        {
            // init graphics device manager and set content root
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Initialize the main application.
        /// </summary>
        protected override void Initialize()
        {
            // create and init the UI manager
            UserInterface.Initialize(Content, BuiltinThemes.hd);
            UserInterface.Active.UseRenderTarget = true;

            // draw cursor outside the render target
            UserInterface.Active.IncludeCursorInRenderTarget = false;

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // make the window fullscreen (but still with border and top control bar)
            int _ScreenWidth = graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Width;
            int _ScreenHeight = graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Height;
            graphics.PreferredBackBufferWidth = _ScreenWidth;
            graphics.PreferredBackBufferHeight = _ScreenHeight;
            graphics.IsFullScreen = false;
            graphics.ApplyChanges();

            // init ui and examples
            InitExamplesAndUI();
        }

        /// <summary>
        /// Show next UI example.
        /// </summary>
        public void NextExample(ref List<Panel> panels)
        {
            currExample++;
            UpdateAfterExampleChange(ref panels);
        }

        /// <summary>
        /// Show previous UI example.
        /// </summary>
        public void PreviousExample(ref List<Panel> panels)
        {
            currExample--;
            UpdateAfterExampleChange(ref panels);
        }

        /// <summary>
        /// Called after we change current example index, to hide all examples
        /// except for the currently active example + disable prev / next buttons if
        /// needed (if first or last example).
        /// </summary>
        protected void UpdateAfterExampleChange(ref List<Panel> panels)
        {
            // hide all panels and show current example panel
            foreach (var panel in panels)
            {
                panel.Visible = false;
            }

            if (currExample < panels.Count)
            {
                panels[currExample].Visible = true;
            }

            // disable / enable next and previous buttons
            nextExampleButton.Enabled = currExample < panels.Count;
        }

        /// <summary>
        /// Create the top bar with next / prev buttons etc, and init all UI example panels.
        /// </summary>        
        private void InitExamplesAndUI()
        {
            // will init examples only if true
            bool initExamples = true;

            var eventsLog = CreateHud();

            // init all examples

            if (initExamples)
            {
                AddWelcomeMessage();
                AddFeaturesList();
                AddBasicConcepts();
                AddAnchors();
                AddButtons();
                AddCheckboxesAndRadioButtons();
                AddPanels();
                AddDraggable();
                AddSliders();
                AddLists();
                AddListAsTables();
                AddListsSkins();
                AddDropdown();

                AddPanelTabs();
                AddPanelsWithScrollbars();
                AddIcons();
                AddTextInput();
                AddTooltipText();
                AddLockedTextInput();
                AddMessages();
                AddFileMenu();
                AddDisabled();
                AddLocked();
                AddCursors();
                AddMisc();
                AddCharacterBuilderPageIntro();
                AddCharacterBuilderPageFinal();
                AddEpilogue();

                // init panels and buttons
                UpdateAfterExampleChange(ref panels);

            }
            // once done init, clear events log
            eventsLog.ClearItems();
        }

        private SelectList CreateHud()
        {
            // create top panel
            int topPanelHeight = 65;
            var topPanel = new Panel(new Vector2(0, topPanelHeight + 2), PanelSkin.Simple, Anchor.TopCenter)
            {
                Padding = Vector2.Zero
            };
            UserInterface.Active.AddEntity(topPanel);

            // add next example button
            nextExampleButton = new Button("Next ->", ButtonSkin.Default, Anchor.TopRight, new Vector2(300, topPanelHeight))
            {
                OnClick = btn => { NextExample(ref panels); },
                Identifier = "next_btn"
            };
            topPanel.AddChild(nextExampleButton);

            // add show-get button
            var showGitButton = new Button("Git Repo", ButtonSkin.Fancy, Anchor.TopCenter, new Vector2(280, topPanelHeight))
            {
                OnClick = btn => { System.Diagnostics.Process.Start("https://github.com/RonenNess/GeonBit.UI"); }
            };
            topPanel.AddChild(showGitButton);

            // add exit button
            var exitBtn = new Button("Exit", anchor: Anchor.BottomRight, size: new Vector2(200, -1))
            {
                OnClick = entity => { Exit(); }
            };
            UserInterface.Active.AddEntity(exitBtn);

            // events panel for debug
            var eventsPanel = new Panel(new Vector2(400, 530), PanelSkin.Simple, Anchor.CenterLeft, new Vector2(-10, 0))
            {
                Visible = false
            };

            // events log (single-time events)
            eventsPanel.AddChild(new Label("Events Log:"));
            var eventsLog = new SelectList(size: new Vector2(-1, 280))
            {
                ExtraSpaceBetweenLines = -8,
                ItemsScale = 0.5f,
                Locked = true
            };
            eventsPanel.AddChild(eventsLog);

            // current events (events that happen while something is true)
            eventsPanel.AddChild(new Label("Current Events:"));
            var eventsNow = new SelectList(size: new Vector2(-1, 100))
            {
                ExtraSpaceBetweenLines = -8,
                ItemsScale = 0.5f,
                Locked = true
            };
            eventsPanel.AddChild(eventsNow);

            // paragraph to show currently active panel
            targetEntityShow = new Paragraph("test", Anchor.Auto, Color.White, scale: 0.75f);
            eventsPanel.AddChild(targetEntityShow);

            // add the events panel
            UserInterface.Active.AddEntity(eventsPanel);

            // whenever events log list size changes, make sure its not too long. if it is, trim it.
            eventsLog.OnListChange = entity =>
            {
                var list = (SelectList)entity;
                if (list.Count > 100)
                {
                    list.RemoveItem(0);
                }
            };

            // listen to all global events - one timers
            UserInterface.Active.OnClick = entity => { eventsLog.AddItem("Click: " + entity.GetType().Name); eventsLog.scrollToEnd(); };
            UserInterface.Active.OnRightClick = entity => { eventsLog.AddItem("RightClick: " + entity.GetType().Name); eventsLog.scrollToEnd(); };
            UserInterface.Active.OnMouseDown = entity => { eventsLog.AddItem("MouseDown: " + entity.GetType().Name); eventsLog.scrollToEnd(); };
            UserInterface.Active.OnRightMouseDown = entity => { eventsLog.AddItem("RightMouseDown: " + entity.GetType().Name); eventsLog.scrollToEnd(); };
            UserInterface.Active.OnMouseEnter = entity => { eventsLog.AddItem("MouseEnter: " + entity.GetType().Name); eventsLog.scrollToEnd(); };
            UserInterface.Active.OnMouseLeave = entity => { eventsLog.AddItem("MouseLeave: " + entity.GetType().Name); eventsLog.scrollToEnd(); };
            UserInterface.Active.OnMouseReleased = entity => { eventsLog.AddItem("MouseReleased: " + entity.GetType().Name); eventsLog.scrollToEnd(); };
            UserInterface.Active.OnMouseWheelScroll = entity => { eventsLog.AddItem("Scroll: " + entity.GetType().Name); eventsLog.scrollToEnd(); };
            UserInterface.Active.OnStartDrag = entity => { eventsLog.AddItem("StartDrag: " + entity.GetType().Name); eventsLog.scrollToEnd(); };
            UserInterface.Active.OnStopDrag = entity => { eventsLog.AddItem("StopDrag: " + entity.GetType().Name); eventsLog.scrollToEnd(); };
            UserInterface.Active.OnFocusChange = entity => { eventsLog.AddItem("FocusChange: " + entity.GetType().Name); eventsLog.scrollToEnd(); };
            UserInterface.Active.OnValueChange = entity => { if (entity.Parent == eventsLog) { return; } eventsLog.AddItem("ValueChanged: " + entity.GetType().Name); eventsLog.scrollToEnd(); };

            // clear the current events after every frame they were drawn
            eventsNow.AfterDraw = entity => { eventsNow.ClearItems(); };

            // listen to all global events - happening now
            UserInterface.Active.WhileDragging = entity => { eventsNow.AddItem("Dragging: " + entity.GetType().Name); eventsNow.scrollToEnd(); };
            UserInterface.Active.WhileMouseDown = entity => { eventsNow.AddItem("MouseDown: " + entity.GetType().Name); eventsNow.scrollToEnd(); };
            UserInterface.Active.WhileMouseHover = entity => { eventsNow.AddItem("MouseHover: " + entity.GetType().Name); eventsNow.scrollToEnd(); };
            eventsNow.MaxItems = 4;

            // add extra info button
            var infoBtn = new Button("  Events", anchor: Anchor.BottomLeft, size: new Vector2(280, -1), offset: new Vector2(140, 0));
            infoBtn.AddChild(new Icon(IconType.Scroll, Anchor.CenterLeft), true);
            infoBtn.OnClick = entity =>
            {
                eventsPanel.Visible = !eventsPanel.Visible;
            };
            infoBtn.ToolTipText = "Show events log.";
            UserInterface.Active.AddEntity(infoBtn);

            // add button to apply transformations
            var transBtn = new Button("Transform UI", anchor: Anchor.BottomLeft, size: new Vector2(320, -1), offset: new Vector2(140 + 280, 0))
            {
                OnClick = entity =>
                {
                    if (UserInterface.Active.RenderTargetTransformMatrix == null)
                    {
                        UserInterface.Active.RenderTargetTransformMatrix = Matrix.CreateScale(0.6f) *
                            Matrix.CreateRotationZ(0.05f) *
                            Matrix.CreateTranslation(new Vector3(150, 150, 0));
                    }
                    else
                    {
                        UserInterface.Active.RenderTargetTransformMatrix = null;
                    }
                },
                ToolTipText = "Apply transform matrix on the entire UI."
            };
            UserInterface.Active.AddEntity(transBtn);

            // zoom in / out factor
            float zoominFactor = 0.05f;

            // scale show
            var scaleShow = new Paragraph("100%", Anchor.BottomLeft, offset: new Vector2(10, 70));
            UserInterface.Active.AddEntity(scaleShow);

            // init zoom-out button
            var zoomout = new Button(string.Empty, ButtonSkin.Default, Anchor.BottomLeft, new Vector2(70, 70));
            var zoomoutIcon = new Icon(IconType.ZoomOut, Anchor.Center, 0.75f);
            zoomout.AddChild(zoomoutIcon, true);
            zoomout.OnClick = btn =>
            {
                if (UserInterface.Active.GlobalScale > 0.5f)
                    UserInterface.Active.GlobalScale -= zoominFactor;
                scaleShow.Text = ((int)System.Math.Round(UserInterface.Active.GlobalScale * 100f)).ToString() + "%";
            };
            UserInterface.Active.AddEntity(zoomout);

            // init zoom-in button
            var zoomin = new Button(string.Empty, ButtonSkin.Default, Anchor.BottomLeft, new Vector2(70, 70), new Vector2(70, 0));
            var zoominIcon = new Icon(IconType.ZoomIn, Anchor.Center, 0.75f);
            zoomin.AddChild(zoominIcon, true);
            zoomin.OnClick = btn =>
            {
                if (UserInterface.Active.GlobalScale < 1.45f)
                    UserInterface.Active.GlobalScale += zoominFactor;
                scaleShow.Text = ((int)System.Math.Round(UserInterface.Active.GlobalScale * 100f)).ToString() + "%";
            };

            topPanel.AddChild(new Paragraph("Cursor-Mod", Anchor.TopLeft, Color.Yellow, offset: new Vector2(10, 7)));
            topPanel.AddChild(new Paragraph("", Anchor.TopLeft, offset: new Vector2(10, 27))
            {
                // Display the UserInterface.Active.GetGamePadMod state
                AfterUpdate = entity => { ((Paragraph)entity).Text = UserInterface.GetCursorMode.ToString(); }
            });

            UserInterface.Active.AddEntity(zoomin);
            return eventsLog;
        }

        private void AddWelcomeMessage()
        {
            // create panel and add to list of panels and manager
            var panel = new Panel(new Vector2(500, 620));
            panels.Add(panel);
            UserInterface.Active.AddEntity(panel);

            // add title and text
            var title = new Image(Content.Load<Texture2D>("example/GeonBitUI-sm"), new Vector2(400, 240), anchor: Anchor.TopCenter, offset: new Vector2(0, -20))
            {
                ShadowColor = new Color(0, 0, 0, 128),
                ShadowOffset = Vector2.One * -6
            };
            panel.AddChild(title);
            var welcomeText = new MulticolorParagraph(@"Welcome to {{RED}}GeonBit{{MAGENTA}}.UI{{DEFAULT}}!
To start the demo, please click the 'Next' button on the top navbar.");
            panel.AddChild(welcomeText);
            panel.AddChild(new Paragraph("V" + UserInterface.VERSION, Anchor.BottomRight)).FillColor = Color.Yellow;

            nextExampleButton = new Button("Next ->", ButtonSkin.Default, size: new Vector2(0, 80)) { Selectable = true };
            nextExampleButton.OnClick = btn => { NextExample(); };
            panel.AddChild(nextExampleButton);
            panel.MakeFirstSelection();
        }

        private void AddFeaturesList()
        {
            // create panel and add to list of panels and manager
            var panel = new Panel(new Vector2(500, 590));
            panels.Add(panel);
            UserInterface.Active.AddEntity(panel);

            // add title and text
            panel.AddChild(new Header("Widget Types"));
            panel.AddChild(new HorizontalLine());
            panel.AddChild(new Paragraph(@"GeonBit.UI implements the following widgets:

- Paragraphs
- Headers
- Buttons
- Panels
- CheckBox
- Radio buttons
- Rectangles
- Images & Icons
- Select List
- Dropdown
- Panel Tabs
- Sliders & Progressbars
- Text input
- Tooltip Text
- And more...
"));

            nextExampleButton = new Button("Next ->", ButtonSkin.Default, size: new Vector2(0, 80)) { Selectable = true };
            nextExampleButton.OnClick = btn => { NextExample(); };
            panel.AddChild(nextExampleButton);
            panel.MakeFirstSelection();
        }

        private void AddBasicConcepts()
        {
            // create panel and add to list of panels and manager
            var panel = new Panel(new Vector2(740, 540));
            panels.Add(panel);
            UserInterface.Active.AddEntity(panel);

            // add title and text
            panel.AddChild(new Header("Basic Concepts"));
            panel.AddChild(new HorizontalLine());
            panel.AddChild(new Paragraph(@"Panels are the basic containers of GeonBit.UI. They are like window forms.

To position elements inside panels or other widgets, you set an anchor and offset. An anchor is a pre-defined position in parent element, like top-left corner, center, etc. and offset is just the distance from that point.

Another thing to keep in mind is size; Most widgets come with a default size, but for those you need to set size for remember that setting size 0 will take full width / height. For example, size of X = 0, Y = 100 means the widget will be 100 pixels height and the width of its parent (minus the parent padding)."));
            nextExampleButton = new Button("Next ->", ButtonSkin.Default, size: new Vector2(0, 80)) { Selectable = true };
            nextExampleButton.OnClick = btn => { NextExample(); };
            panel.AddChild(nextExampleButton);
            panel.MakeFirstSelection();
        }

        private void AddAnchors()
        {
            // create panel and add to list of panels and manager
            var panel = new Panel(new Vector2(800, 620));
            panels.Add(panel);
            UserInterface.Active.AddEntity(panel);

            // add title and text
            panel.AddChild(new Paragraph(@"Anchors help position elements. For example, this paragraph anchor is 'center'.

The most common anchors are 'Auto' and 'AutoInline', which will place entities one after another automatically.",
                Anchor.Center, Color.White, 0.8f, new Vector2(320, 0)));

            panel.AddChild(new Header("Anchors", Anchor.TopCenter, new Vector2(0, 100)));
            panel.AddChild(new Paragraph("top-left", Anchor.TopLeft, Color.Yellow, 0.8f));
            panel.AddChild(new Paragraph("top-center", Anchor.TopCenter, Color.Yellow, 0.8f));
            panel.AddChild(new Paragraph("top-right", Anchor.TopRight, Color.Yellow, 0.8f));
            panel.AddChild(new Paragraph("bottom-left", Anchor.BottomLeft, Color.Yellow, 0.8f));
            panel.AddChild(new Paragraph("bottom-center", Anchor.BottomCenter, Color.Yellow, 0.8f));
            panel.AddChild(new Paragraph("bottom-right", Anchor.BottomRight, Color.Yellow, 0.8f));
            panel.AddChild(new Paragraph("center-left", Anchor.CenterLeft, Color.Yellow, 0.8f));
            panel.AddChild(new Paragraph("center-right", Anchor.CenterRight, Color.Yellow, 0.8f));
            nextExampleButton = new Button("Next ->", ButtonSkin.Default, Anchor.BottomRight, size: new Vector2(0, 80)) { Selectable = true };
            nextExampleButton.OnClick = btn => { NextExample(); };
            panel.AddChild(nextExampleButton);
            panel.MakeFirstSelection();
        }

        private void AddButtons()
        {
            // create panel and add to list of panels and manager
            var panel = new Panel(new Vector2(450, 735));
            panels.Add(panel);
            UserInterface.Active.AddEntity(panel);

            // add title and text
            panel.AddChild(new Header("Buttons"));
            panel.AddChild(new HorizontalLine());
            panel.AddChild(new Paragraph(
@"Please press [DPAD-Down] or [DPAD-Up] to navigate between the Buttons."));

            // add default button            
            var custom0 = new Button("Default", ButtonSkin.Default, size: new Vector2(0, 80)) { Selectable = true };
            custom0.Identifier = "First Button";
            panel.AddChild(custom0);

            // alternative button
            var custom1 = new Button("Alternative", ButtonSkin.Alternative, size: new Vector2(0, 80)) { Selectable = true };
            custom1.Identifier = "Second Button";
            panel.AddChild(custom1);

            // fancy button
            var custom2 = new Button("Fancy", ButtonSkin.Fancy, size: new Vector2(0, 80)) { Selectable = true };
            custom2.Identifier = "Third Button";
            panel.AddChild(custom2);

            // custom button
            var custom3 = new Button("Custom", ButtonSkin.Default, size: new Vector2(0, 80)) { Selectable = true };
            custom3.SetCustomSkin(
                Content.Load<Texture2D>("example/btn_default"),
                Content.Load<Texture2D>("example/btn_hover"),
                Content.Load<Texture2D>("example/btn_down"));
            custom3.Identifier = "Fourth Button";
            panel.AddChild(custom3);

            // toggle button
            panel.AddChild(new LineSpace());
            panel.AddChild(new HorizontalLine());
            panel.AddChild(new LineSpace());
            panel.AddChild(new Paragraph("This text is not selectable! Press [A] Button to perform a click event."));
            var btn = new Button("Toggle Me!", ButtonSkin.Default) { Selectable = true };
            btn.ToggleMode = true;
            btn.Identifier = "Toggle Button";
            panel.AddChild(btn);

            // add next example button
            nextExampleButton = new Button("Next ->", ButtonSkin.Default, size: new Vector2(0, 80)) { Selectable = true };
            nextExampleButton.OnClick = nxtButton => { NextExample(); };
            panel.AddChild(nextExampleButton);
            panel.MakeFirstSelection();
        }

        private void AddCheckboxesAndRadioButtons()
        {
            // create panel and add to list of panels and manager
            var panel = new Panel(new Vector2(450, 520));
            panels.Add(panel);
            UserInterface.Active.AddEntity(panel);

            // checkboxes example
            panel.AddChild(new Header("CheckBox"));
            panel.AddChild(new HorizontalLine());
            panel.AddChild(new Paragraph("CheckBoxes example:"));

            panel.AddChild(new CheckBox("CheckBox 1") { Selectable = true });
            panel.AddChild(new CheckBox("CheckBox 2") { Selectable = true });

            // radio example
            panel.AddChild(new LineSpace(3));
            panel.AddChild(new Header("Radio buttons"));
            panel.AddChild(new HorizontalLine());
            panel.AddChild(new Paragraph("Radio buttons example:"));

            panel.AddChild(new RadioButton("Option 1") { Selectable = true });
            panel.AddChild(new RadioButton("Option 2") { Selectable = true });
            panel.AddChild(new RadioButton("Option 3") { Selectable = true });
            nextExampleButton = new Button("Next ->", ButtonSkin.Default, size: new Vector2(0, 80)) { Selectable = true };
            nextExampleButton.OnClick = btn => { NextExample(); };
            panel.AddChild(nextExampleButton);
            panel.MakeFirstSelection();
        }

        private void AddPanels()
        {
            // create panel and add to list of panels and manager
            var panel = new Panel(new Vector2(450, 640));
            panels.Add(panel);
            UserInterface.Active.AddEntity(panel);

            // title and text
            panel.AddChild(new Header("Panels"));
            panel.AddChild(new HorizontalLine());
            panel.AddChild(new Paragraph("GeonBit.UI comes with 4 alternative panel skins:"));
            int panelHeight = 110;
            {
                var intPanel = new Panel(new Vector2(0, panelHeight), PanelSkin.Fancy, Anchor.Auto) { Selectable = true };
                intPanel.AddChild(new Paragraph("Fancy Panel", Anchor.Center));
                panel.AddChild(intPanel);
            }
            {
                var intPanel = new Panel(new Vector2(0, panelHeight), PanelSkin.Golden, Anchor.Auto) { Selectable = true };
                intPanel.AddChild(new Paragraph("Golden Panel", Anchor.Center));
                panel.AddChild(intPanel);
            }
            {
                var intPanel = new Panel(new Vector2(0, panelHeight), PanelSkin.Simple, Anchor.Auto) { Selectable = true };
                intPanel.AddChild(new Paragraph("Simple Panel", Anchor.Center));
                panel.AddChild(intPanel);
            }
            {
                var intPanel = new Panel(new Vector2(0, panelHeight), PanelSkin.ListBackground, Anchor.Auto) { Selectable = true };
                intPanel.AddChild(new Paragraph("List Background", Anchor.Center));
                panel.AddChild(intPanel);
            }
            nextExampleButton = new Button("Next ->", ButtonSkin.Default, size: new Vector2(0, 80)) { Selectable = true };
            nextExampleButton.OnClick = btn => { NextExample(); };
            panel.AddChild(nextExampleButton);
            panel.MakeFirstSelection();
        }

        private void AddDraggable()
        {
            // create panel and add to list of panels and manager
            var panel = new Panel(new Vector2(450, 690))
            {
                Draggable = true
            };
            panels.Add(panel);
            UserInterface.Active.AddEntity(panel);

            // title and text
            panel.AddChild(new Header("Draggable"));
            panel.AddChild(new HorizontalLine());
            panel.AddChild(new Paragraph("This panel can be dragged, try it out!"));
            panel.AddChild(new LineSpace());
            panel.AddChild(new HorizontalLine());
            panel.AddChild(new LineSpace());
            var paragraph = new Paragraph("Any type of entity can be dragged. For example, try to drag this text!");
            paragraph.SetStyleProperty("FillColor", new StyleProperty(Color.Yellow));
            paragraph.SetStyleProperty("FillColor", new StyleProperty(Color.Purple), EntityState.MouseHover);
            paragraph.Draggable = true;
            paragraph.LimitDraggingToParentBoundaries = false;
            panel.AddChild(paragraph);

            // internal panel with internal draggable
            var panelInt = new Panel(new Vector2(250, 250), PanelSkin.Golden, Anchor.AutoCenter)
            {
                Draggable = true
            };
            nextExampleButton = new Button("Next ->", ButtonSkin.Default, size: new Vector2(0, 80)) { Selectable = true };
            nextExampleButton.OnClick = btn => { NextExample(); };
            panelInt.AddChild(new Paragraph("This panel is draggable too, but limited to its parent boundaries.", Anchor.Center, Color.White, 0.85f));
            panel.AddChild(panelInt);
            panel.AddChild(nextExampleButton);
            panel.MakeFirstSelection();
        }

        private void AddSliders()
        {
            // create panel and add to list of panels and manager
            var panel = new Panel(new Vector2(450, 540));
            panels.Add(panel);
            UserInterface.Active.AddEntity(panel);

            // sliders title
            panel.AddChild(new Header("Sliders"));
            panel.AddChild(new HorizontalLine());
            panel.AddChild(new Paragraph("Sliders help pick numeric value in range, hold A to drag:"));

            panel.AddChild(new Paragraph("\nDefault slider"));
            panel.AddChild(new Slider(0, 10, SliderSkin.Default) { Selectable = true });

            panel.AddChild(new Paragraph("\nFancy slider"));
            panel.AddChild(new Slider(0, 10, SliderSkin.Fancy) { Selectable = true });

            // progressbar title
            panel.AddChild(new LineSpace(3));
            panel.AddChild(new Header("Progress bar"));
            panel.AddChild(new HorizontalLine());
            panel.AddChild(new Paragraph("Works just like sliders:"));
            panel.AddChild(new ProgressBar(0, 10) { Selectable = true });

            nextExampleButton = new Button("Next ->", ButtonSkin.Default, size: new Vector2(0, 80)) { Selectable = true };
            nextExampleButton.OnClick = btn => { NextExample(); };
            panel.AddChild(nextExampleButton);
            panel.MakeFirstSelection();
        }

        private void AddLists()
        {
            // create panel and add to list of panels and manager
            var panel = new Panel(new Vector2(450, 460));
            panels.Add(panel);
            UserInterface.Active.AddEntity(panel);

            // list title
            panel.AddChild(new Header("SelectList"));
            panel.AddChild(new HorizontalLine());
            panel.AddChild(new Paragraph("SelectLists let you pick a value from a list of items:"));

            var list = new SelectList(new Vector2(0, 280));
            list.AddItem("Warrior");
            list.AddItem("Mage");
            list.AddItem("Ranger");
            list.AddItem("Rogue");
            list.AddItem("Paladin");
            list.AddItem("Cleric");
            list.AddItem("Warlock");
            list.AddItem("Barbarian");
            list.AddItem("Monk");
            list.AddItem("Ranger");
            panel.AddChild(list);
            nextExampleButton = new Button("Next ->", ButtonSkin.Default, size: new Vector2(0, 80)) { Selectable = true };
            nextExampleButton.OnClick = btn => { NextExample(); };
            panel.AddChild(nextExampleButton);
            panel.MakeFirstSelection();
        }

        private void AddListAsTables()
        {
            // create panel and add to list of panels and manager
            var panel = new Panel(new Vector2(620, 460));
            panels.Add(panel);
            UserInterface.Active.AddEntity(panel);

            // list title
            panel.AddChild(new Header("SelectList as a Table"));
            panel.AddChild(new HorizontalLine());
            panel.AddChild(new Paragraph("With few simple tricks you can also create lists that behave like a table:"));

            // create the list
            var list = new SelectList(new Vector2(0, 280));

            // lock and create title
            list.LockedItems[0] = true;
            list.AddItem(string.Format("{0}{1,-8} {2,-8} {3, -10}", "{{RED}}", "Name", "Class", "Level"));

            // add items as formatted table
            list.AddItem(string.Format("{0,-8} {1,-8} {2,-10}", "Joe", "Mage", "5"));
            list.AddItem(string.Format("{0,-8} {1,-8} {2,-10}", "Ron", "Monk", "7"));
            list.AddItem(string.Format("{0,-8} {1,-8} {2,-10}", "Alex", "Rogue", "3"));
            list.AddItem(string.Format("{0,-8} {1,-8} {2,-10}", "Jim", "Paladin", "7"));
            list.AddItem(string.Format("{0,-8} {1,-8} {2,-10}", "Abe", "Cleric", "8"));
            list.AddItem(string.Format("{0,-8} {1,-8} {2,-10}", "James", "Warlock", "20"));
            list.AddItem(string.Format("{0,-8} {1,-8} {2,-10}", "Bob", "Bard", "1"));
            panel.AddChild(list);
            nextExampleButton = new Button("Next ->", ButtonSkin.Default, size: new Vector2(0, 80)) { Selectable = true };
            nextExampleButton.OnClick = btn => { NextExample(); };
            panel.AddChild(nextExampleButton);
            panel.MakeFirstSelection();
        }

        private void AddListsSkins()
        {
            // create panel and add to list of panels and manager
            var panel = new Panel(new Vector2(450, 460));
            panels.Add(panel);
            UserInterface.Active.AddEntity(panel);

            // list title
            panel.AddChild(new Header("SelectList - Skin"));
            panel.AddChild(new HorizontalLine());
            panel.AddChild(new Paragraph("Just like panels, SelectList can use alternative skins:"));

            var list = new SelectList(new Vector2(0, 280), skin: PanelSkin.Golden);
            list.AddItem("Warrior");
            list.AddItem("Mage");
            list.AddItem("Ranger");
            list.AddItem("Rogue");
            list.AddItem("Paladin");
            list.AddItem("Cleric");
            list.AddItem("Warlock");
            list.AddItem("Barbarian");
            list.AddItem("Monk");
            list.AddItem("Ranger");
            panel.AddChild(list);
            nextExampleButton = new Button("Next ->", ButtonSkin.Default, size: new Vector2(0, 80)) { Selectable = true };
            nextExampleButton.OnClick = btn => { NextExample(); };
            panel.AddChild(nextExampleButton);
            panel.MakeFirstSelection();
        }

        private void AddDropdown()
        {
            // create panel and add to list of panels and manager
            var panel = new Panel(new Vector2(450, 430));
            panels.Add(panel);
            UserInterface.Active.AddEntity(panel);

            // dropdown title
            panel.AddChild(new Header("DropDown"));
            panel.AddChild(new HorizontalLine());

            panel.AddChild(new Paragraph("DropDown is just like a list, but take less space since it hide the list when not used:"));
            var drop = new DropDown(new Vector2(0, 250));
            drop.AddItem("Warrior");
            drop.AddItem("Mage");
            drop.AddItem("Ranger");
            drop.AddItem("Rogue");
            drop.AddItem("Paladin");
            drop.AddItem("Cleric");
            drop.AddItem("Warlock");
            drop.AddItem("Barbarian");
            drop.AddItem("Monk");
            drop.AddItem("Ranger");
            panel.AddChild(drop);

            panel.AddChild(new Paragraph("And like list, we can set different skins:"));
            drop = new DropDown(new Vector2(0, 180), skin: PanelSkin.Golden);
            drop.AddItem("Warrior");
            drop.AddItem("Mage");
            drop.AddItem("Monk");
            drop.AddItem("Ranger");
            panel.AddChild(drop);
            nextExampleButton = new Button("Next ->", ButtonSkin.Default, size: new Vector2(0, 80)) { Selectable = true };
            nextExampleButton.OnClick = btn => { NextExample(); };
            panel.AddChild(nextExampleButton);
            panel.MakeFirstSelection();
        }

        private void AddPanelsWithScrollbars()
        {
            // create panel and add to list of panels and manager
            var panel = new Panel(new Vector2(450, 440)) { Selectable = true };
            panels.Add(panel);
            UserInterface.Active.AddEntity(panel);

            // dropdown title
            panel.AddChild(new Header("Panel Overflow"));
            panel.AddChild(new HorizontalLine());
            panel.AddChild(new Paragraph(@"You can choose how to handle entities that overflow parent panel's boundaries. 

The default behavior is to simply overflow (eg entities will be drawn as usual), but you can also make overflowing entities clip, or make the entire panel scrollable. 

In this example, we use a panel with scrollbars.

Note that in order to use clipping and scrollbar with Panels you need to set the UserInterface.Active.UseRenderTarget flag to true.

Here's a button, to test clicking while scrolled:"));
            panel.AddChild(new Button("a button.") { Selectable = true });
            panel.AddChild(new Paragraph(@"And here's a dropdown:"));
            var dropdown = new DropDown(new Vector2(0, 220));
            for (int i = 1; i < 10; ++i) dropdown.AddItem("Option" + i.ToString());
            panel.AddChild(dropdown);
            panel.AddChild(new Paragraph(@"And a list:"));
            var list = new SelectList(new Vector2(0, 220));
            for (int i = 1; i < 10; ++i) list.AddItem("Option" + i.ToString());
            panel.AddChild(list);
            panel.PanelOverflowBehavior = PanelOverflowBehavior.VerticalScroll;
            panel.Scrollbar.AdjustMaxAutomatically = true;
            panel.Identifier = "panel_with_scrollbar";
            nextExampleButton = new Button("Next ->", ButtonSkin.Default, size: new Vector2(0, 80)) { Selectable = true };
            nextExampleButton.OnClick = btn => { NextExample(); };
            panel.AddChild(nextExampleButton);
            panel.MakeFirstSelection();
        }

        private void AddIcons()
        {
            // create panel and add to list of panels and manager
            var panel = new Panel(new Vector2(460, 640));
            panels.Add(panel);
            UserInterface.Active.AddEntity(panel);

            // icons title
            panel.AddChild(new Header("Icons"));
            panel.AddChild(new HorizontalLine());
            panel.AddChild(new Paragraph("GeonBit.UI comes with some built-in icons:"));

            foreach (IconType icon in System.Enum.GetValues(typeof(IconType)))
            {
                if (icon == IconType.None)
                {
                    continue;
                }
                panel.AddChild(new Icon(icon, Anchor.AutoInline));
            }

            panel.AddChild(new Paragraph("And you can also add an inventory-like frame:"));
            panel.AddChild(new LineSpace());
            for (int i = 0; i < 6; ++i)
            {
                panel.AddChild(new Icon((IconType)i, Anchor.AutoInline, 1, true));
            }

            nextExampleButton = new Button("Next ->", ButtonSkin.Default, size: new Vector2(0, 80)) { Selectable = true };
            nextExampleButton.OnClick = btn => { NextExample(); };
            panel.AddChild(nextExampleButton);
            panel.MakeFirstSelection();
        }

        private void AddTextInput()
        {
            // create panel and add to list of panels and manager
            var panel = new Panel(new Vector2(450, 700));
            panels.Add(panel);
            UserInterface.Active.AddEntity(panel);

            // text input example
            panel.AddChild(new Header("Text Input"));
            panel.AddChild(new HorizontalLine());

            // inliner
            panel.AddChild(new Paragraph("Text input let you get free text from the user:"));
            var text = new TextInput(false)
            {
                PlaceholderText = "Insert text.."
            };
            panel.AddChild(text);

            // multiline
            panel.AddChild(new Paragraph("Text input can also be multiline, and use different panel skins:"));
            var textMulti = new TextInput(true, new Vector2(0, 220), skin: PanelSkin.Golden)
            {
                PlaceholderText = @"Insert multiline text.."
            };
            panel.AddChild(textMulti);

            // with hidden password chars
            panel.AddChild(new Paragraph("Hidden text input:"));
            var hiddenText = new TextInput(false)
            {
                PlaceholderText = "Enter password..",
                HideInputWithChar = '*'
            };
            panel.AddChild(hiddenText);
            var hideCheckbox = new CheckBox("Hide password", isChecked: true) { Selectable = true };
            hideCheckbox.OnValueChange += ent =>
            {
                if (hideCheckbox.Checked)
                    hiddenText.HideInputWithChar = '*';
                else
                    hiddenText.HideInputWithChar = null;
            };
            panel.AddChild(hideCheckbox);

            nextExampleButton = new Button("Next ->", ButtonSkin.Default, size: new Vector2(0, 80)) { Selectable = true };
            nextExampleButton.OnClick = btn => { NextExample(); };
            panel.AddChild(nextExampleButton);
            panel.MakeFirstSelection();
        }

        private void AddTooltipText()
        {
            // create panel and add to list of panels and manager
            var panel = new Panel(new Vector2(450, 550));
            panels.Add(panel);
            UserInterface.Active.AddEntity(panel);

            // text input example
            panel.AddChild(new Header("Tooltip Text"));
            panel.AddChild(new HorizontalLine());

            // add entity with tooltip text
            panel.AddChild(new Paragraph(@"You can attach tooltip text to entities.
This text will be shown when the user points on the entity for few seconds. 

For example, try to point on this button:"));
            var tooltipButton = new Button("Button With Tooltip")
            {
                ToolTipText = @"This is the button tooltip text!
And yes, it can be multiline."
            };
            panel.AddChild(tooltipButton);
            panel.AddChild(new Paragraph(@"Note that you can override the function that generates tooltip text entities if you want to create your own custom style."));
            nextExampleButton = new Button("Next ->", ButtonSkin.Default, size: new Vector2(0, 80)) { Selectable = true };
            nextExampleButton.OnClick = btn => { NextExample(); };
            panel.AddChild(nextExampleButton);
            panel.MakeFirstSelection();
        }

        private void AddLockedTextInput()
        {
            // create panel and add to list of panels and manager
            var panel = new Panel(new Vector2(500, 570));
            panels.Add(panel);
            UserInterface.Active.AddEntity(panel);

            // text input example
            panel.AddChild(new Header("Locked Text Input"));
            panel.AddChild(new HorizontalLine());

            // inliner
            panel.AddChild(new Paragraph("A locked multiline text is a cool trick to create long, scrollable text:"));
            var textMulti = new TextInput(true, new Vector2(0, 370))
            {
                Locked = true
            };
            textMulti.TextParagraph.Scale = 0.6f;
            textMulti.Value = @"The Cleric, Priest, or Bishop is a character class in Dungeons & Dragons and other fantasy role-playing games. 

The cleric is a healer, usually a priest and a holy warrior, originally modeled on or inspired by the Military Orders. 
Clerics are usually members of religious orders, with the original intent being to portray soldiers of sacred orders who have magical abilities, although this role was later taken more clearly by the paladin. 

Most clerics have powers to heal wounds, protect their allies and sometimes resurrect the dead, as well as summon, manipulate and banish undead.

A description of Priests and Priestesses from the Nethack guidebook: Priests and Priestesses are clerics militant, crusaders advancing the cause of righteousness with arms, armor, and arts thaumaturgic. Their ability to commune with deities via prayer occasionally extricates them from peril, but can also put them in it.[1]

A common feature of clerics across many games is that they may not equip pointed weapons such as swords or daggers, and must use blunt weapons such as maces, war-hammers, shields or wand instead. This is based on a popular, but erroneous, interpretation of the depiction of Odo of Bayeux and accompanying text. They are also often limited in what types of armor they can wear, though usually not as restricted as mages.

Related to the cleric is the paladin, who is typically a Lawful Good[citation needed] warrior often aligned with a religious order, and who uses their martial skills to advance its holy cause.";
            panel.AddChild(textMulti);
            nextExampleButton = new Button("Next ->", ButtonSkin.Default, size: new Vector2(0, 80)) { Selectable = true };
            nextExampleButton.OnClick = btn => { NextExample(); };
            panel.AddChild(nextExampleButton);
            panel.MakeFirstSelection();
        }

        private void AddPanelTabs()
        {
            // create panel and add to list of panels and manager
            var panel = new Panel(new Vector2(600, 520));
            panels.Add(panel);
            UserInterface.Active.AddEntity(panel);

            // create panel tabs
            var tabs = new PanelTabs();
            panel.AddChild(tabs);

            // add first panel
            {
                var tab = tabs.AddTab("Tab 1");
                tab.panel.AddChild(new Header("Sliders"));
                tab.panel.AddChild(new HorizontalLine());
                tab.panel.AddChild(new Paragraph("Press the [A] Button and then drag the mark with the [Left ThumbStick]."));

                tab.panel.AddChild(new Paragraph("\nSlider"));

                // entry point for gamepad tabbing
                tab.panel.AddChild(new Slider(0, 10, SliderSkin.Default)
                { Selectable = true, AttachedData = tab.button });

                tab.panel.AddChild(new Paragraph("\nPress the [DPAD-Left] or [DPAD-Right] Button to select a different Tab!"));

                // entry point for gamepad tabbing
                tab.panel.AddChild(new Slider(0, 10, SliderSkin.Fancy)
                { Selectable = true, AttachedData = tab.button });

                tab.panel.AddChild(new LineSpace(3));
                tab.panel.AddChild(new Header("Progress Bar"));
                tab.panel.AddChild(new HorizontalLine());

                // entry point for gamepad tabbing
                tab.panel.AddChild(new ProgressBar(0, 10) { Selectable = true, AttachedData = tab.button });

                tab.button.Selectable = true;   // a tab should be selectable by the gamepad
                tab.button.AttachedData = tabs; // attach the PanelTabs to the button for further navigation
                tab.panel.MakeFirstSelection(); // mark the first selectable entity on the panel
            }

            // add second panel
            {
                var tab = tabs.AddTab("Tab 2");
                tab.panel.AddChild(new Header("Tab 2"));
                tab.panel.AddChild(new HorizontalLine());
                tab.panel.AddChild(new Paragraph(@"
Press the [DPAD-Down] or [DPAD-Up] Button to select an entity in the panel!"));

                // checkboxes example
                tab.panel.AddChild(new Header("CheckBox"));
                tab.panel.AddChild(new HorizontalLine());
                tab.panel.AddChild(new Paragraph("CheckBoxes example:", anchor: Anchor.CenterLeft));

                tab.panel.AddChild(new CheckBox("CheckBox 1", anchor: Anchor.CenterLeft, size: new Vector2(300, -1), offset: new Vector2(0, 40))
                { Selectable = true, AttachedData = tab.button /*entry point for gamepad tabbing*/ });
                tab.panel.AddChild(new CheckBox("CheckBox 2", anchor: Anchor.CenterRight, size: new Vector2(300, -1), offset: new Vector2(0, 40))
                { Selectable = true, AttachedData = tab.button /*entry point for gamepad tabbing*/ });

                // radio example
                tab.panel.AddChild(new LineSpace(3));
                tab.panel.AddChild(new Header("Radio buttons"));
                tab.panel.AddChild(new HorizontalLine());
                tab.panel.AddChild(new Paragraph("Radio buttons example:", anchor: Anchor.BottomLeft, offset: new Vector2(0, 60)));

                tab.panel.AddChild(new RadioButton("Option 1", anchor: Anchor.BottomLeft, size: new Vector2(200, -1), offset: new Vector2(0, 0))
                { Selectable = true, AttachedData = tab.button /*entry point for gamepad tabbing*/ });
                tab.panel.AddChild(new RadioButton("Option 2", anchor: Anchor.BottomLeft, size: new Vector2(200, -1), offset: new Vector2(180, 0))
                { Selectable = true, AttachedData = tab.button /*entry point for gamepad tabbing*/ });
                tab.panel.AddChild(new RadioButton("Option 3", anchor: Anchor.BottomLeft, size: new Vector2(200, -1), offset: new Vector2(360, 0))
                { Selectable = true, AttachedData = tab.button /*entry point for gamepad tabbing*/ });

                tab.button.Selectable = true;   // a tab should be selectable by the gamepad
                tab.button.AttachedData = tabs; // attach the PanelTabs to the button for further navigation
                tab.panel.MakeFirstSelection(); // mark the first selectable entity on the panel
            }

            // add third panel
            {
                var tab = tabs.AddTab("Tab 3");
                tab.panel.AddChild(new Header("Tab 3"));
                tab.panel.AddChild(new HorizontalLine());
                tab.panel.AddChild(new Paragraph(@"When it's not possible to detect selectable entities in the panel, then the mouse cursor will be automatically 
            bound to the current tab button, so it's still possible to navigate between the tabs."));

                // add disabled button
                nextExampleButton = new Button("Disabled Button", ButtonSkin.Fancy, size: new Vector2(0, 80))
                { Selectable = true, AttachedData = tab.button, Enabled = false /*entry point for gamepad tabbing*/  };
                tab.panel.AddChild(nextExampleButton);

                tab.button.Selectable = true;   // a tab should be selectable by the gamepad
                tab.button.AttachedData = tabs; // attach the PanelTabs to the button for further navigation
                tab.panel.MakeFirstSelection(); // mark the first selectable entity on the panel
            }

            // add fourth panel
            {
                var tab = tabs.AddTab("Tab 4");
                tab.panel.AddChild(new Header("Tab 4"));
                tab.panel.AddChild(new HorizontalLine());
                tab.panel.AddChild(new Paragraph(@"This example showed you the 'snapping-mode'.

            In the next example you will see the 'roaming-mode'. There you can move the mouse cursor freely with your [Left Thumbstick].

            This allows you to do more complex menu navigation."));

                // add next example button
                nextExampleButton = new Button("Next ->", ButtonSkin.Default, size: new Vector2(0, 80))
                { Selectable = true, AttachedData = tab.button  /*entry point for gamepad tabbing*/ };
                nextExampleButton.OnClick = btn2 => { NextExample(); };
                tab.panel.AddChild(nextExampleButton);

                tab.button.Selectable = true;   // a tab should be selectable by the gamepad
                tab.button.AttachedData = tabs; // attach the PanelTabs to the button for further navigation
                tab.panel.MakeFirstSelection(); // mark the first selectable entity on the panel
            }
        }

        private void AddMessages()
        {
            // create panel and add to list of panels and manager
            var panel = new Panel(new Vector2(450, 560)) { Identifier = "" };
            panels.Add(panel);
            UserInterface.Active.AddEntity(panel);

            // add title and text
            panel.AddChild(new Header("Message Box"));
            panel.AddChild(new HorizontalLine());
            panel.AddChild(new Paragraph("GeonBit.UI comes with a utility to create simple message boxes:"));

            // button to create simple message box
            {
                var btn = new Button("Show Simple Message", ButtonSkin.Default) { Selectable = true };
                btn.OnClick += (GeonBit.UI.Entities.Entity entity) =>
                {
                    var messagePanel = GeonBit.UI.Utils.MessageBox.ShowMsgBox("Hello World!", "This is a simple message box. It doesn't say much, really.");
                };
                panel.AddChild(btn);
            }

            // button to create message box with custombuttons
            panel.AddChild(new Paragraph("Or you can create custom message and buttons:"));
            {
                var btn = new Button("Show Custom Message", ButtonSkin.Default) { Selectable = true };
                btn.OnClick += (GeonBit.UI.Entities.Entity entity) =>
                {
                    GeonBit.UI.Utils.MessageBox.ShowMsgBox("Custom Message!", "In this message there are two custom buttons.\n\nYou can set different actions per button. For example, click on 'Surprise' and see what happens!", new GeonBit.UI.Utils.MessageBox.MsgBoxOption[] {
                                new GeonBit.UI.Utils.MessageBox.MsgBoxOption("Close", () => { return true; }),
                                new GeonBit.UI.Utils.MessageBox.MsgBoxOption("Surprise", () => { GeonBit.UI.Utils.MessageBox.ShowMsgBox("Files Removed Successfully", "Win32 was successfully removed from this computer. Please restart to complete OS destruction.\n\n(Just kidding!)"); return true; })
                                });
                };
                panel.AddChild(btn);
            }

            // button to create message with extras
            panel.AddChild(new Paragraph("And you can also add extra entities to the message box:"));
            {
                var btn = new Button("Message With Extras", ButtonSkin.Default) { Selectable = true };
                btn.OnClick += entity =>
                {
                    var textInput = new TextInput(false)
                    {
                        PlaceholderText = "Enter your name"
                    };
                    GeonBit.UI.Utils.MessageBox.ShowMsgBox("Message With Extra!", "In this message box we attached an extra entity from outside (a simple text input).\n\nPretty neat, huh?",
                        new GeonBit.UI.Utils.MessageBox.MsgBoxOption[] {
                                new GeonBit.UI.Utils.MessageBox.MsgBoxOption("Close", () => { return true; }),
                        }, new UIEntity[] { textInput });
                };
                panel.AddChild(btn);
            }

            nextExampleButton = new Button("Next ->", ButtonSkin.Default, size: new Vector2(0, 80)) { Selectable = true };
            nextExampleButton.OnClick = btn => { NextExample(); };
            panel.AddChild(nextExampleButton);
            panel.MakeFirstSelection();
        }

        private void AddFileMenu()
        {
            // create panel and add to list of panels and manager
            var panel = new Panel(new Vector2(750, 660));
            panels.Add(panel);
            UserInterface.Active.AddEntity(panel);

            // add title and text
            panel.AddChild(new Header("File Menu"));
            panel.AddChild(new HorizontalLine());
            panel.AddChild(new Paragraph("GeonBit.UI comes with a utility to generate a classic file menu:"));

            var layout = new GeonBit.UI.Utils.SimpleFileMenu.MenuLayout();
            layout.AddMenu("File", 260);
            layout.AddItemToMenu("File", "New", () => { GeonBit.UI.Utils.MessageBox.ShowMsgBox("Something New!", "Lets make something new."); });
            layout.AddItemToMenu("File", "Save", () => { GeonBit.UI.Utils.MessageBox.ShowMsgBox("Something Saved!", "Your thing was saved successfully."); });
            layout.AddItemToMenu("File", "Load", () => { GeonBit.UI.Utils.MessageBox.ShowMsgBox("Something Loaded!", "Your thing was loaded successfully."); });
            layout.AddItemToMenu("File", "Exit", () => { GeonBit.UI.Utils.MessageBox.ShowMsgBox("Not Yet", "We still have much to see."); });
            layout.AddMenu("Display", 260);
            layout.AddItemToMenu("Display", "Zoom In", () => { UserInterface.Active.GlobalScale += 0.1f; });
            layout.AddItemToMenu("Display", "Zoom Out", () => { UserInterface.Active.GlobalScale -= 0.1f; });
            layout.AddItemToMenu("Display", "Reset Zoom", () => { UserInterface.Active.GlobalScale = 1f; });
            var fileMenu = GeonBit.UI.Utils.SimpleFileMenu.Create(layout);
            fileMenu.SetAnchor(Anchor.Auto);
            panel.AddChild(fileMenu);
            panel.AddChild(new LineSpace(24));

            panel.AddChild(new Paragraph("Usually this menu should cover the top of the screen and not be inside another panel. Note that like most entities in GeonBit.UI, you can also set its skin:"));
            fileMenu = GeonBit.UI.Utils.SimpleFileMenu.Create(layout, PanelSkin.Fancy);
            fileMenu.SetAnchor(Anchor.Auto);
            panel.AddChild(fileMenu);

            nextExampleButton = new Button("Next ->", ButtonSkin.Default, size: new Vector2(0, 80)) { Selectable = true };
            nextExampleButton.OnClick = btn => { NextExample(); };
            panel.AddChild(nextExampleButton);
            panel.MakeFirstSelection();
        }

        private void AddDisabled()
        {
            // create panel and add to list of panels and manager
            var panel = new Panel(new Vector2(480, 580));
            panels.Add(panel);
            UserInterface.Active.AddEntity(panel);

            // disabled title
            panel.AddChild(new Header("Disabled"));
            panel.AddChild(new HorizontalLine());
            panel.AddChild(new Paragraph("Entities can be disabled:"));

            // internal panel
            var panel2 = new Panel(Vector2.Zero, PanelSkin.None, Anchor.Auto)
            {
                Padding = Vector2.Zero
            };
            panel.AddChild(panel2);
            panel2.AddChild(new Button("button") { Enabled = false });

            panel2.AddChild(new LineSpace());
            for (int i = 0; i < 6; ++i)
            {
                panel2.AddChild(new Icon((IconType)i, Anchor.AutoInline, 1, true) { Enabled = false });
            }
            panel2.AddChild(new Paragraph("\nDisabled entities are drawn in black & white, and you cannot interact with them.."));

            var list = new SelectList(new Vector2(0, 130)) { Enabled = false };
            list.AddItem("Warrior");
            list.AddItem("Mage");
            panel2.AddChild(list);
            panel2.AddChild(new CheckBox("disabled..") { Enabled = false });

            nextExampleButton = new Button("Next ->", ButtonSkin.Default, size: new Vector2(0, 80)) { Selectable = true };
            nextExampleButton.OnClick = btn => { NextExample(); };
            panel2.AddChild(nextExampleButton);
            panel2.MakeFirstSelection();
        }

        private void AddLocked()
        {
            // create panel and add to list of panels and manager
            var panel = new Panel(new Vector2(520, 610));
            panels.Add(panel);
            UserInterface.Active.AddEntity(panel);

            // locked title
            panel.AddChild(new Header("Locked"));
            panel.AddChild(new HorizontalLine());
            panel.AddChild(new Paragraph("Entities can also be locked:", Anchor.Auto));

            var panel2 = new Panel(Vector2.Zero, PanelSkin.None, Anchor.Auto)
            {
                Padding = Vector2.Zero
            };

            panel.AddChild(panel2);
            panel2.AddChild(new Button("button") { Locked = true });
            panel2.AddChild(new LineSpace());

            for (int i = 0; i < 6; ++i)
            {
                panel2.AddChild(new Icon((IconType)i, Anchor.AutoInline, 1, true));
            }
            panel2.AddChild(new Paragraph("\nLocked entities will not respond to input, but unlike disabled entities they are drawn normally, eg with colors:"));

            var list = new SelectList(new Vector2(0, 130)) { Locked = true };
            list.AddItem("Warrior");
            list.AddItem("Mage");
            panel2.AddChild(list);
            panel2.AddChild(new CheckBox("locked..") { Locked = true });

            nextExampleButton = new Button("Next ->", ButtonSkin.Default, size: new Vector2(0, 80)) { Selectable = true };
            nextExampleButton.OnClick = btn => { NextExample(); };
            panel2.AddChild(nextExampleButton);
            panel2.MakeFirstSelection();
        }

        private void AddCursors()
        {
            // create panel and add to list of panels and manager
            var panel = new Panel(new Vector2(450, 540));
            panels.Add(panel);
            UserInterface.Active.AddEntity(panel);

            // add title and text
            panel.AddChild(new Header("Cursor"));
            panel.AddChild(new HorizontalLine());
            panel.AddChild(new Paragraph("GeonBit.UI comes with 3 basic cursor types:"));

            // default cursor show
            {
                var btn = new Button("Default", ButtonSkin.Default) { Selectable = true };
                btn.OnMouseEnter = entity => { UserInterface.Active.SetCursor(CursorType.Default); };
                btn.OnMouseLeave = entity => { UserInterface.Active.SetCursor(CursorType.Default); };
                panel.AddChild(btn);
            }

            // pointer cursor show
            {
                var btn = new Button("Pointer", ButtonSkin.Default) { Selectable = true };
                btn.OnMouseEnter = entity => { UserInterface.Active.SetCursor(CursorType.Pointer); };
                btn.OnMouseLeave = entity => { UserInterface.Active.SetCursor(CursorType.Default); };
                panel.AddChild(btn);
            }

            // ibeam cursor show
            {
                var btn = new Button("IBeam", ButtonSkin.Default) { Selectable = true };
                btn.OnMouseEnter = entity => { UserInterface.Active.SetCursor(CursorType.IBeam); };
                btn.OnMouseLeave = entity => { UserInterface.Active.SetCursor(CursorType.Default); };
                panel.AddChild(btn);
            }

            panel.AddChild(new Paragraph("And as always, you can also set your own custom cursor:"));
            {
                var btn = new Button("Custom", ButtonSkin.Default) { Selectable = true };
                btn.OnMouseEnter = entity => { UserInterface.Active.SetCursor(Content.Load<Texture2D>("example/cursor"), 40); };
                btn.OnMouseLeave = entity => { UserInterface.Active.SetCursor(CursorType.Default); };
                panel.AddChild(btn);
            }

            nextExampleButton = new Button("Next ->", ButtonSkin.Default, size: new Vector2(0, 80)) { Selectable = true };
            nextExampleButton.OnClick = btn => { NextExample(); };
            panel.AddChild(nextExampleButton);
            panel.MakeFirstSelection();
        }

        private void AddMisc()
        {
            // create panel and add to list of panels and manager
            var panel = new Panel(new Vector2(530, 590));
            panels.Add(panel);
            UserInterface.Active.AddEntity(panel);

            // misc title
            panel.AddChild(new Header("Miscellaneous"));
            panel.AddChild(new HorizontalLine());
            panel.AddChild(new Paragraph("Some cool tricks you can do:"));

            // button with icon
            var buttonWithIcon = new Button("Button With Icon");
            buttonWithIcon.ButtonParagraph.SetPosition(Anchor.CenterLeft, new Vector2(60, 0));
            buttonWithIcon.AddChild(new Icon(IconType.Book, Anchor.CenterLeft), true);
            panel.AddChild(buttonWithIcon);

            // change progressbar color
            panel.AddChild(new Paragraph("Different ProgressBar colors:"));
            var pb = new ProgressBar();
            pb.ProgressFill.FillColor = Color.Red;
            pb.Caption.Text = "Optional caption...";
            panel.AddChild(pb);

            // paragraph style with mouse
            panel.AddChild(new LineSpace());
            panel.AddChild(new HorizontalLine());
            var paragraph = new Paragraph("Hover / click styling..");
            paragraph.SetStyleProperty("FillColor", new StyleProperty(Color.Purple), EntityState.MouseDown);
            paragraph.SetStyleProperty("FillColor", new StyleProperty(Color.Red), EntityState.MouseHover);
            panel.AddChild(paragraph);
            panel.AddChild(new HorizontalLine());

            // colored rectangle
            panel.AddChild(new Paragraph("Colored rectangle:"));
            var rect = new ColoredRectangle(Color.Blue, Color.Red, 4, new Vector2(0, 40));
            panel.AddChild(rect);
            panel.AddChild(new HorizontalLine());

            // custom icons
            panel.AddChild(new Paragraph("Custom icons / images:"));
            var icon = new Icon(IconType.None, Anchor.AutoInline, 1, true, new Vector2(12, 10))
            {
                Texture = Content.Load<Texture2D>("example/warrior")
            };
            panel.AddChild(icon);
            icon = new Icon(IconType.None, Anchor.AutoInline, 1, true, new Vector2(12, 10))
            {
                Texture = Content.Load<Texture2D>("example/monk")
            };
            panel.AddChild(icon);
            icon = new Icon(IconType.None, Anchor.AutoInline, 1, true, new Vector2(12, 10))
            {
                Texture = Content.Load<Texture2D>("example/mage")
            };
            panel.AddChild(icon);

            nextExampleButton = new Button("Next ->", ButtonSkin.Default, size: new Vector2(0, 80)) { Selectable = true };
            nextExampleButton.OnClick = btn => { NextExample(); };
            panel.AddChild(nextExampleButton);
            panel.MakeFirstSelection();
        }

        private void AddColumnToggleExample()
        {
            int panelWidth = 730;

            // create panel and add to list of panels and manager
            var panel = new Panel(new Vector2(panelWidth, 550));
            panels.Add(panel);
            UserInterface.Active.AddEntity(panel);

            // create an internal panel to align components better - a row that covers the entire width split into 3 columns (left, center, right)
            // first the container panel
            var entitiesGroup = new Panel(new Vector2(0, 240), PanelSkin.None, Anchor.AutoCenter)
            {
                Padding = Vector2.Zero
            };
            panel.AddChild(entitiesGroup);

            // left side
            var leftPanel = new Panel(new Vector2(0.45f, 500), PanelSkin.None, Anchor.TopLeft)
            {
                Padding = Vector2.Zero,
                PanelOverflowBehavior = PanelOverflowBehavior.VerticalScroll,
                Identifier = "leftpanel_with_scrollbar"
            };
            leftPanel.Scrollbar.AdjustMaxAutomatically = true;
            leftPanel.AddChild(new Header("Left Side"));
            entitiesGroup.AddChild(leftPanel);

            // right side
            var rightPanel = new Panel(new Vector2(0.45f, 0), PanelSkin.None, Anchor.TopRight)
            {
                Padding = Vector2.Zero
            };
            rightPanel.AddChild(new Header("Right Side"));
            entitiesGroup.AddChild(rightPanel);

            var leftButton = new Button("LeftButton", ButtonSkin.Default)
            {
                OnClick = myBtn =>
                {
                    if (myBtn.Parent == leftPanel)
                    {
                        leftPanel.RemoveChild(myBtn);
                        rightPanel.AddChild(myBtn);
                    }
                    else
                    {
                        rightPanel.RemoveChild(myBtn);
                        leftPanel.AddChild(myBtn);
                    }
                    panel.MarkAsDirty();
                },
                Selectable = true
            };
            leftPanel.AddChild(leftButton);

            var rightButton = new Button("RightButton", ButtonSkin.Default)
            {
                OnClick = myBtn =>
                {
                    if (myBtn.Parent == leftPanel)
                    {
                        leftPanel.RemoveChild(myBtn);
                        rightPanel.AddChild(myBtn);
                    }
                    else
                    {
                        rightPanel.RemoveChild(myBtn);
                        leftPanel.AddChild(myBtn);
                    }
                    panel.MarkAsDirty();
                },
                Selectable = true
            };
            rightPanel.AddChild(rightButton);
            panel.MakeFirstSelection();
            leftPanel.MakeFirstSelection();
        }

        private void AddCharacterBuilderPageIntro()
        {
            // create panel and add to list of panels and manager
            var panel = new Panel(new Vector2(500, 300));
            panels.Add(panel);
            UserInterface.Active.AddEntity(panel);

            // add title and text
            panel.AddChild(new Header("Final Example"));
            panel.AddChild(new HorizontalLine());
            panel.AddChild(new Paragraph(@"The next example will show a fully-functional character creation page, that use different entities, events, etc.

Click on 'Next' to see the character creation demo."));

            nextExampleButton = new Button("Next ->", ButtonSkin.Default, size: new Vector2(0, 80)) { Selectable = true };
            nextExampleButton.OnClick = btn => { NextExample(); };
            panel.AddChild(nextExampleButton);
            panel.MakeFirstSelection();
        }

        private void AddCharacterBuilderPageFinal()
        {
            int panelWidth = 730;

            // create panel and add to list of panels and manager
            var panel = new Panel(new Vector2(panelWidth, 550));
            panels.Add(panel);
            UserInterface.Active.AddEntity(panel);

            // add title and text
            panel.AddChild(new Header("Create New Character"));
            panel.AddChild(new HorizontalLine());

            // create an internal panel to align components better - a row that covers the entire width split into 3 columns (left, center, right)
            // first the container panel
            var entitiesGroup = new Panel(new Vector2(0, 240), PanelSkin.None, Anchor.AutoCenter)
            {
                Padding = Vector2.Zero
            };
            panel.AddChild(entitiesGroup);

            // now left side
            var leftPanel = new Panel(new Vector2(0.33f, 0), PanelSkin.None, Anchor.TopLeft)
            {
                Padding = Vector2.Zero
            };
            entitiesGroup.AddChild(leftPanel);

            // right side
            var rightPanel = new Panel(new Vector2(0.33f, 0), PanelSkin.None, Anchor.TopRight)
            {
                Padding = Vector2.Zero
            };
            entitiesGroup.AddChild(rightPanel);

            // center
            var centerPanel = new Panel(new Vector2(0.33f, 0), PanelSkin.None, Anchor.TopCenter)
            {
                Padding = Vector2.Zero
            };
            entitiesGroup.AddChild(centerPanel);

            // create a character preview panel
            centerPanel.AddChild(new Label(@"Preview", Anchor.AutoCenter));
            var charPreviewPanel = new Panel(new Vector2(180, 180), PanelSkin.Simple, Anchor.AutoCenter)
            {
                Padding = Vector2.Zero
            };
            centerPanel.AddChild(charPreviewPanel);

            // create preview pics of character
            var previewImage = new Image(Content.Load<Texture2D>("example/warrior"), Vector2.Zero, anchor: Anchor.Center);
            var previewImageColor = new Image(Content.Load<Texture2D>("example/warrior_color"), Vector2.Zero, anchor: Anchor.Center);
            var previewImageSkin = new Image(Content.Load<Texture2D>("example/warrior_skin"), Vector2.Zero, anchor: Anchor.Center);
            charPreviewPanel.AddChild(previewImage);
            charPreviewPanel.AddChild(previewImageColor);
            charPreviewPanel.AddChild(previewImageSkin);

            // add skin tone slider
            var skin = new Slider(0, 10, new Vector2(0, -1), SliderSkin.Default, Anchor.Auto)
            {
                OnValueChange = entity =>
                {
                    var slider = (Slider)entity;
                    int alpha = (int)(slider.GetValueAsPercent() * 255);
                    previewImageSkin.FillColor = new Color(60, 32, 25, alpha);
                },
                Value = 5
            };
            charPreviewPanel.AddChild(skin);

            // create the class selection list
            leftPanel.AddChild(new Label(@"Class", Anchor.AutoCenter));
            var classTypes = new SelectList(new Vector2(0, 208), Anchor.Auto);
            classTypes.AddItem("Warrior");
            classTypes.AddItem("Mage");
            classTypes.AddItem("Ranger");
            classTypes.AddItem("Monk");
            classTypes.SelectedIndex = 0;
            leftPanel.AddChild(classTypes);
            classTypes.OnValueChange = entity =>
            {
                string texture = ((SelectList)(entity)).SelectedValue.ToLower();
                previewImage.Texture = Content.Load<Texture2D>("example/" + texture);
                previewImageColor.Texture = Content.Load<Texture2D>("example/" + texture + "_color");
                previewImageSkin.Texture = Content.Load<Texture2D>("example/" + texture + "_skin");
            };

            // create color selection buttons
            rightPanel.AddChild(new Label(@"Color", Anchor.AutoCenter));
            Color[] colors = { Color.White, Color.Red, Color.Green, Color.Blue, Color.Yellow, Color.Purple, Color.Cyan, Color.Brown };
            int colorPickSize = 24;
            foreach (var baseColor in colors)
            {
                rightPanel.AddChild(new LineSpace(0));
                for (int i = 0; i < 8; ++i)
                {
                    var color = baseColor * (1.0f - (i * 2 / 16.0f)); color.A = 255;
                    var currColorButton = new ColoredRectangle(color, Vector2.One * colorPickSize, Anchor.AutoInline);
                    currColorButton.Padding = currColorButton.SpaceAfter = currColorButton.SpaceBefore = Vector2.Zero;
                    currColorButton.OnClick = entity =>
                    {
                        previewImageColor.FillColor = entity.FillColor;
                    };
                    rightPanel.AddChild(currColorButton);
                }
            }

            // gender selection (radio buttons)
            entitiesGroup.AddChild(new LineSpace());
            entitiesGroup.AddChild(new RadioButton("Male", Anchor.Auto, new Vector2(180, 60), isChecked: true));
            entitiesGroup.AddChild(new RadioButton("Female", Anchor.AutoInline, new Vector2(240, 60)));

            // hardcore mode
            var hardcore = new Button("Hardcore", ButtonSkin.Fancy, Anchor.AutoInline, new Vector2(220, 60));
            hardcore.ButtonParagraph.Scale = 0.8f;
            hardcore.ToggleMode = true;
            entitiesGroup.AddChild(hardcore);
            entitiesGroup.AddChild(new HorizontalLine());

            // add character name, last name, and age
            // first add the labels
            entitiesGroup.AddChild(new Label(@"First Name: ", Anchor.AutoInline, size: new Vector2(0.4f, -1)));
            entitiesGroup.AddChild(new Label(@"Last Name: ", Anchor.AutoInline, size: new Vector2(0.4f, -1)));
            entitiesGroup.AddChild(new Label(@"Age: ", Anchor.AutoInline, size: new Vector2(0.2f, -1)));

            // now add the text inputs

            // first name
            var firstName = new TextInput(false, new Vector2(0.4f, -1), anchor: Anchor.Auto)
            {
                PlaceholderText = "Name"
            };
            firstName.Validators.Add(new TextValidatorEnglishCharsOnly(true));
            firstName.Validators.Add(new OnlySingleSpaces());
            firstName.Validators.Add(new TextValidatorMakeTitle());
            entitiesGroup.AddChild(firstName);

            // last name
            var lastName = new TextInput(false, new Vector2(0.4f, -1), anchor: Anchor.AutoInline)
            {
                PlaceholderText = "Surname"
            };
            lastName.Validators.Add(new TextValidatorEnglishCharsOnly(true));
            lastName.Validators.Add(new OnlySingleSpaces());
            lastName.Validators.Add(new TextValidatorMakeTitle());
            entitiesGroup.AddChild(lastName);

            // age
            var age = new TextInput(false, new Vector2(0.2f, -1), anchor: Anchor.AutoInline);
            age.Validators.Add(new TextValidatorNumbersOnly(false, 0, 80));
            age.Value = "20";
            age.ValueWhenEmpty = "20";
            entitiesGroup.AddChild(age);

            nextExampleButton = new Button("Next ->", ButtonSkin.Default, size: new Vector2(0, 80)) { Selectable = true };
            nextExampleButton.OnClick = btn => { NextExample(); };
            panel.AddChild(nextExampleButton);
            panel.MakeFirstSelection();
        }

        private void AddEpilogue()
        {
            // create panel and add to list of panels and manager
            var panel = new Panel(new Vector2(520, 400));
            panels.Add(panel);
            UserInterface.Active.AddEntity(panel);

            // add title and text
            panel.AddChild(new Header("End Of Demo"));
            panel.AddChild(new HorizontalLine());
            panel.AddChild(new Paragraph(@"That's it for now! There is still much to learn about GeonBit.UI, but these examples were enough to get you going.

To learn more, please visit the git repo, read the docs, or go through some source code.

If you liked GeonBit.UI feel free to star the repo on GitHub. :)"));
        }

        /// <summary>
        /// Show next UI example.
        /// </summary>
        public void NextExample()
        {
            currExample++;
            UpdateAfterExampleChange();
        }

        /// <summary>
        /// Show previous UI example.
        /// </summary>
        public void PreviousExample()
        {
            currExample--;
            UpdateAfterExampleChange();
        }

        /// <summary>
        /// Called after we change current example index, to hide all examples
        /// except for the currently active example + disable prev / next buttons if
        /// needed (if first or last example).
        /// </summary>
        protected void UpdateAfterExampleChange()
        {
            // hide all panels and show current example panel
            foreach (var panel in panels)
            {
                panel.Visible = false;
            }
            panels[currExample].Visible = true;

            // disable / enable next and previous buttons
            nextExampleButton.Enabled = currExample < panels.Count;
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // make sure window is focused
            if (!IsActive)
                return;

            // exit on escape
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            // update UI
            UserInterface.Active.Update(gameTime);

            // show currently active entity (for testing)
            targetEntityShow.Text = "Target Entity: " + (UserInterface.Active.TargetEntity != null ? UserInterface.Active.TargetEntity.GetType().Name : "null");

            // call base update
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            // draw ui
            UserInterface.Active.Draw(spriteBatch);

            // clear buffer
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // finalize ui rendering
            UserInterface.Active.DrawMainRenderTarget(spriteBatch, new Rectangle(0, 0, 1920, 1080));

            // call base draw function
            base.Draw(gameTime);
        }
    }
}
