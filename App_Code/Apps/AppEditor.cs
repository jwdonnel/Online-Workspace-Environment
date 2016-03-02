using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;

namespace TextEditor {
    /// <summary>
    /// TinyMCE Control for ASP.Net
    /// </summary>
    public class AppEditor : TextBox {
        public AppEditor() { }

        protected override void OnPreRender(EventArgs e) {
            string tinyMceIncludeKey = "TinyMCEInclude";
            string tinyMceIncludeScript = "<script type=\"text/javascript\" src=\"//tinymce.cachefly.net/4.1/tinymce.min.js\"></script>";

            if (!Page.ClientScript.IsStartupScriptRegistered(tinyMceIncludeKey)) {
                Page.ClientScript.RegisterStartupScript(this.GetType(), tinyMceIncludeKey, tinyMceIncludeScript);
            }

            if (!Page.ClientScript.IsStartupScriptRegistered(GetInitKey())) {
                Page.ClientScript.RegisterStartupScript(this.GetType(), GetInitKey(), GetInitScript());
            }

            if (!CssClass.Contains(GetEditorClass())) {
                if (CssClass.Length > 0) {
                    CssClass += " ";
                }
                CssClass += GetEditorClass();
            }
            base.OnPreRender(e);
        }

        private string GetInitKey() {
            string simpleKey = "TinyMCESimple";
            string fullKey = "TinyMCEFull";

            switch (Mode) {
                case TextEditorMode.Simple:
                    return simpleKey;
                case TextEditorMode.Full:
                    return fullKey;
                default:
                    goto case TextEditorMode.Simple;
            }
        }

        private string GetEditorClass() {
            return GetEditorClass(Mode);
        }

        private string GetEditorClass(TextEditorMode mode) {
            string simpleClass = "SimpleTextEditor";
            string fullClass = "FullTextEditor";

            switch (mode) {
                case TextEditorMode.Simple:
                    return simpleClass;
                case TextEditorMode.Full:
                    return fullClass;
                default:
                    goto case TextEditorMode.Simple;
            }
        }

        private string GetInitScript() {
            string simpleScript =
                "<script language=\"javascript\" type=\"text/javascript\">LoadTinyMCEControls_Simple('" + ID + "');</script>";
            string fullScript =
                "<script language=\"javascript\" type=\"text/javascript\">LoadTinyMCEControls_Full('" + ID + "');</script>";

            switch (Mode) {
                case TextEditorMode.Simple:
                    return string.Format(simpleScript, GetEditorClass(TextEditorMode.Simple));
                case TextEditorMode.Full:
                    return string.Format(fullScript, GetEditorClass(TextEditorMode.Full));
                default:
                    goto case TextEditorMode.Simple;
            }
        }

        public override TextBoxMode TextMode {
            get {
                return TextBoxMode.MultiLine;
            }
        }

        public TextEditorMode Mode {
            get {
                Object obj = ViewState["Mode"];
                if (obj == null) {
                    return TextEditorMode.Simple;
                }
                return (TextEditorMode)obj;
            }
            set {
                ViewState["Mode"] = value;
            }
        }

        public enum TextEditorMode {
            Simple,
            Full
        }
    }
}