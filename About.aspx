<%@ Page Title="About" Language="C#" AutoEventWireup="true" CodeFile="About.aspx.cs" Inherits="SiteTools_About" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <meta name="author" content="John Donnelly" />
    <meta name="revisit-after" content="10 days" />
    <meta http-equiv="content-type" content="text/html;charset=utf-8" />
    <meta name="viewport" content="initial-scale=1.0, user-scalable=no" />
    <meta name="apple-mobile-web-app-capable" content="yes" />
    <meta name="mobile-web-app-capable" content="yes" />
    <link id="Link1" runat="server" rel="shortcut icon" href="Standard_Images/favicon.ico"
        type="image/x-icon" />
    <link id="Link2" runat="server" rel="icon" href="Standard_Images/favicon.ico" type="image/ico" />
    <link href="App_Themes/Standard/site_desktop.css" rel="stylesheet" type="text/css" />
    <link rel="stylesheet" href="App_Themes/Standard/jqueryUI.css" />
    <style type="text/css">
        #siteInfo ul
        {
            list-style: none;
        }

        #siteInfo li
        {
            padding: 2px 5px;
        }

        .logdates
        {
            /* font-weight: bold; */
            text-align: center;
            width: 90px;
            border-right: 1px solid #DFDFDF;
            border-bottom: 1px solid #DFDFDF;
            color: #888;
            background: #FCFCFC;
        }

        .logentry
        {
            border-bottom: 1px solid #DFDFDF;
        }

        #changeLog
        {
            font-size: 14px;
        }

        #siteInfo, #termsofuse, #changeLog, #helppages
        {
            padding: 20px 35px;
            line-height: normal;
            position: relative;
        }

        #siteInfo, #termsofuse, #changeLog, #helppage-select-holder, #forkmeInfo, .pnlLinkBtns_Holder
        {
            margin: 0 auto;
            max-width: 1000px;
        }

        #lbl_currentVer
        {
            font-weight: bold;
        }

        .termsofuse-section
        {
            padding-bottom: 30px;
            clear: both;
        }

        .termsofus-section-title
        {
            font-weight: bold;
            font-size: 14px;
            clear: both;
            padding-bottom: 10px;
        }

        .termsofus-section-text
        {
            clear: both;
            line-height: 18px !important;
            font-size: 13px !important;
        }

        .screenshot-img, .workspace-img
        {
            max-height: 450px;
            max-width: 100%;
            -moz-box-shadow: 0 2px 4px rgba(0,0,0,.12);
            -o-box-shadow: 0 2px 4px rgba(0,0,0,.12);
            -webkit-box-shadow: 0 2px 4px rgba(0,0,0,.12);
            box-shadow: 0 2px 4px rgba(0,0,0,.12);
        }

        .version-img
        {
            max-height: 100px;
            -moz-box-shadow: 0 2px 4px rgba(0,0,0,.12);
            -o-box-shadow: 0 2px 4px rgba(0,0,0,.12);
            -webkit-box-shadow: 0 2px 4px rgba(0,0,0,.12);
            box-shadow: 0 2px 4px rgba(0,0,0,.12);
        }

        .about-section
        {
            padding-bottom: 60px;
            line-height: 22px;
            font-size: 14px;
        }

        .lnk_Download
        {
            border: 1px solid #3079ED;
            background: #4D90FE;
            float: left;
            -webkit-border-radius: 5px;
            -moz-border-radius: 5px;
            border-radius: 5px;
            -webkit-transition: all .2s ease-in-out;
            -moz-transition: all .2s ease-in-out;
            transition: all .2s ease-in-out;
        }

            .lnk_Download .download-text
            {
                color: #FFF;
                font-size: 14px;
                font-weight: bold;
                padding: 10px 15px;
                float: left;
            }

            .lnk_Download:hover
            {
                background: #357AE8;
                boder: 1px solid #2F5BB7;
            }

            .lnk_Download:active
            {
                background: #2B65BC;
                border: 1px solid #234589;
            }

        #forkme_banner
        {
            float: left;
            z-index: 10;
            padding: 10px 50px 10px 10px;
            color: #FFF;
            background: url('Standard_Images/About Logos/blacktocat.png') #0090FF no-repeat 95% 50%;
            font-weight: 700;
            box-shadow: 0 0 10px rgba(0, 0, 0, 0.5);
            border-bottom-left-radius: 2px;
            border-bottom-right-radius: 2px;
            text-decoration: none;
            text-shadow: none;
            transition: color 0.5s ease;
            transition: text-shadow 0.5s ease;
            -webkit-transition: color 0.5s ease;
            -webkit-transition: text-shadow 0.5s ease;
            -moz-transition: color 0.5s ease;
            -moz-transition: text-shadow 0.5s ease;
            -o-transition: color 0.5s ease;
            -o-transition: text-shadow 0.5s ease;
            -ms-transition: color 0.5s ease;
            -ms-transition: text-shadow 0.5s ease;
        }

        #pnlLinkBtns
        {
            padding-left: 35px;
            padding-right: 40px;
            border-bottom: 1px solid #BBB;
            background: -moz-linear-gradient(top,rgba(232,232,232,0) 0,rgba(232,232,232,.65) 100%);
            background: -webkit-gradient(linear,left top,left bottom,color-stop(0,rgba(232, 232, 232, 0)),color-stop(100%,rgba(232, 232, 232, 0.65)));
            background: -webkit-linear-gradient(top,rgba(232, 232, 232, 0) 0,rgba(232, 232, 232, 0.65) 100%);
            background: -o-linear-gradient(top,rgba(232,232,232,0) 0,rgba(232,232,232,.65) 100%);
            background: -ms-linear-gradient(top,rgba(232,232,232,0) 0,rgba(232,232,232,.65) 100%);
            background: linear-gradient(to bottom,rgba(232, 232, 232, 0) 0,rgba(232, 232, 232, 0.65) 100%);
            filter: progid:DXImageTransform.Microsoft.gradient( startColorstr='#00e8e8e8',endColorstr='#a6e8e8e8',GradientType=0 );
        }

            #pnlLinkBtns ul.sitemenu-selection
            {
                background: transparent!important;
                border-bottom: 0px!important;
            }

        .help-intro-text li
        {
            color: #CCC;
        }

        .mobile-pnlLinkBtns
        {
            background: none!important;
            border-bottom: none!important;
        }

        .mobile-siteInfo .about-section
        {
            padding-bottom: 30px !important;
            line-height: 18px !important;
            font-size: 13px !important;
        }

        .mobile-termsofuse .about-section
        {
            padding-bottom: 30px !important;
            line-height: 18px !important;
            font-size: 13px !important;
        }

        .mobile-changeLog
        {
            padding: 0!important;
            font-size: 12px !important;
        }

            .mobile-changeLog table
            {
                border-spacing: 0px;
                border-collapse: collapse;
            }

            .mobile-changeLog .pad-all
            {
                padding: 5px !important;
            }

        .aspNetDisabled
        {
            cursor: default !important;
            background: #CCC !important;
            border: #AAA !important;
        }

            .aspNetDisabled .download-text
            {
                color: #F1F1F1 !important;
                text-shadow: 1px 2px 2px #BBB !important;
            }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager" runat="server" EnablePartialRendering="True"
            EnableHistory="true" EnableSecureHistoryState="False" AsyncPostBackTimeout="360000" />
        <div id="always-visible" style="display: none;">
            <div id="top-main-bar-top">
                <div id="iframe-container-close-btn">
                    <a href="~/Workspace.aspx" runat="server">Close Content</a>
                </div>
            </div>
        </div>
        <div id="app_title_bg" runat="server" class="app-Settings-title-bg-color-main">
            <div class="pad-all">
                <div class="app-Settings-title-user-info">
                    <div class="float-left">
                        <span id="page_title" runat="server" class="page-title">Loading...</span>
                    </div>
                </div>
            </div>
        </div>
        <div class="clear" style="height: 25px;">
        </div>
        <asp:Panel ID="pnlLinkBtns" runat="server">
            <div class="pnlLinkBtns_Holder">
                <ul class="sitemenu-selection">
                    <li id="hdl1" onclick="$('#termsofuse, #changeLog, #helppages').hide();$('#siteInfo').show();$('.sitemenu-selection').find('li').removeClass('active');$('.sitemenu-selection').find('li').eq(0).addClass('active');" class="active"><a href="#" onclick="return false;">About Site</a></li>
                    <li id="hdl2" onclick="$('#siteInfo, #changeLog, #helppages').hide();$('#termsofuse').show();$('.sitemenu-selection').find('li').removeClass('active');$('.sitemenu-selection').find('li').eq(1).addClass('active');"><a href="#" onclick="return false;">Terms of Use</a></li>
                    <li id="hdl3" onclick="$('#siteInfo, #termsofuse, #helppages').hide();$('#changeLog').show();$('.sitemenu-selection').find('li').removeClass('active');$('.sitemenu-selection').find('li').eq(2).addClass('active');"><a href="#" onclick="return false;">Change Log</a></li>
                    <li id="hdl4" onclick="$('#siteInfo, #termsofuse, #changeLog').hide();$('#helppages').show();$('.sitemenu-selection').find('li').removeClass('active');$('.sitemenu-selection').find('li').eq(3).addClass('active');"><a href="#" onclick="return false;">Help Pages</a></li>
                </ul>
            </div>
        </asp:Panel>
        <div class="clear-space">
        </div>
        <div class="clear-space">
        </div>
        <div id="forkmeInfo" class="pad-top-big">
            <a id="forkme_banner" href="https://github.com/jwdonnel/OpenWSE" class="margin-left" target="_blank">View on GitHub</a>
            <div id="lbl_currentVer" runat="server" class="float-right pad-top-big pad-right">
            </div>
        </div>
        <div class="clear-space">
        </div>
        <div id="siteInfo">
            <div id="AboutopenWSE">
                <div class="about-section">
                    <img alt="workspace" src="Standard_Images/About Logos/Workspace.jpg" style="max-width: 100%;" />
                    <div class="clear-space"></div>
                    <div class="clear-space"></div>
                    <h2>Introduction</h2>
                    <div class="clear-space-five"></div>
                    OpenWSE is a Windows desktop like workspace. The goal of this site is to provide a Windows like experience but from your web browser on any computer. Much like your desktop, you can have multiple modal windows (called apps) opened on your workspace. And just like your desktop, if you have apps opened that you dont want to close, but dont want to show, you can simply minimize them in the taskbar at the top. Apps can be based off of UserControls (.ascx), existing webpages, or just custom html pages. Apps can incorporate their own css stylesheets along with javascript files and other code. This allows for any developer to integrate with the OpenWSE code. Each user can register an account (if enabled by the Administrator) that allows you to save your apps position, size, and whats loaded so you can go from any browser or computer and keep the same settings.
                </div>
                <div class="about-section">
                    <h2>Features</h2>
                    <div class="clear-space-five"></div>
                    <ul style="list-style: disc; padding-left: 20px;">
                        <li><b>Apps</b> that can integrate with other sites or with the OpenWSE code built into the site.</li>
                        <li><b>Site Plugins</b> that allow you to upload enhancements that you can make yourself.</li>
                        <li><b>Overlays</b> that are like apps but are more stationary and are only available with certain apps.</li>
                        <li>Install/Remove any app as you please. If available, the <b>App Installer</b> can allow you to install new apps and/or plugins.</li>
                        <li><b>Database Integration and Custom Table builder</b> - If you have another database you want to integrate with, you can use the simple Table Importer tool under the site settings. Furthermore, you can also create your own custom tables that are stored in the sites database. Imports and Custom tables create their own apps for you to interact with.</li>
                        <li>Custom just about anything on your workspace from the background to the taskbar. Make your workspace different from everyone else.</li>
                        <li><b>Chat Client</b> that allows you to live chat with other users. (All chat sessions are encrypted and cannot be read within the database)</li>
                        <li><b>Groups</b> that you can join that allow you to install different apps and chat with users in that group.</li>
                        <li>The <b>App Remote</b> allows you to access your account from your phone or tablet and control your workspace like a remote control.</li>
                        <li>Customize the entire website to your business or personal site.</li>
                        <li>Live updating through apps and notifications.</li>
                        <li>Easily create your own apps and overlays.</li>
                        <li>Features are always being developed and added to enhance the user experience. Check the Change Log for news on the latest changes.</li>
                    </ul>
                </div>
                <div class="about-section">
                    <h2>How can you use this site</h2>
                    <div class="clear-space-five"></div>
                    <ul style="list-style: disc; padding-left: 20px;">
                        <li>As an internal business site</li>
                        <li>Personal social network</li>
                        <li>Your own personal development tool for creating user controls and viewing</li>
                    </ul>
                </div>
                <div class="about-section">
                    <h2>Login Page</h2>
                    <div class="clear-space-five"></div>
                    <ul style="list-style: disc; padding-left: 20px;">
                        <li>The login page can be configured just like anything else on the site.</li>
                        <li>Choose different themes.</li>
                        <li>Enable/Disable user regististration, demo site, and password recovery.</li>
                        <li>Allow users to login using there Google, Twitter, or Facebook account.</li>
                        <li>You can also choose to bypass the login screen and go straight to the workspace as a guest user.</li>
                    </ul>
                    <div class="clear-space"></div>
                    <div style="text-align: center">
                        <img alt="Login Page" src="Standard_Images/About Logos/loginpage.jpg" class="screenshot-img" />
                    </div>
                </div>
                <div class="about-section">
                    <h2>The Workspace</h2>
                    <div class="clear-space-five"></div>
                    <ul style="list-style: disc; padding-left: 20px;">
                        <li>The workspace works like your desktop computer. You can open, close, minimize, maximize, resize and move apps across the workspace.</li>
                        <li>You can choose the number of workspaces you want to show.</li>
                        <li>Apps can be opened to specific workspaces.</li>
                        <li>Each workspace can have its own background. (If enabled)</li>
                        <li>You can switch between workspaces by selecting the
                            <img alt="" src="App_Themes/Standard/Icons/workspace.png" />
                            Workspace at the top left corner of the screen or by pressing Alt + Up Key/Down Key</li>
                        <li>You can also toggle the overlays by selecting the
                            <img alt="" src="App_Themes/Standard/Icons/overlay.png" />
                            or by pressing Alt + O</li>
                    </ul>
                    <div class="clear-space"></div>
                    <div style="text-align: center">
                        <img alt="Workspace" src="Standard_Images/About Logos/openwse_v4.6.jpg" class="screenshot-img" />
                        <div class="clear" style="height: 20px;"></div>
                        <img alt="Workspace" src="Standard_Images/About Logos/openwse_v4.6_overlays.jpg" class="screenshot-img" />
                    </div>
                    <div class="clear-space"></div>
                </div>
                <div class="about-section">
                    <h2>Apps</h2>
                    <div class="clear-space-five"></div>
                    <ul style="list-style: disc; padding-left: 20px;">
                        <li>Apps can be based on a variety of different web technology.</li>
                        <li>Each app can be customized with its own icon, description, theme, and extra settings.</li>
                    </ul>
                    <div class="clear-space"></div>
                    <b>Standard Apps Available:</b>
                    <div class="clear-space-two"></div>
                    <div class="pad-bottom pad-left">
                        <ul class="float-left pad-right-big" style="list-style: none;">
                            <li>1.) FileDrive</li>
                            <li>2.) Bookmark Viewer</li>
                            <li>3.) Sticky Note</li>
                            <li>4.) My Calendar</li>
                            <li>5.) Chat Settings</li>
                            <li>6.) Stock Viewer</li>
                        </ul>
                        <ul class="float-left pad-left-big" style="list-style: none;">
                            <li>7.) Alarm Clock</li>
                            <li>8.) Google Traffic</li>
                            <li>9.) RSS News Feed</li>
                            <li>10.) Twitter Station</li>
                            <li>11.) Message Board</li>
                        </ul>
                    </div>
                    <div class="clear-space"></div>
                </div>
                <div class="about-section">
                    <h2>Site Tools</h2>
                    <div class="clear-space-five"></div>
                    <ul style="list-style: disc; padding-left: 20px;">
                        <li>The site tools give you the ability to customize the entire site to your liking.</li>
                        <li>There are 20 different site tools to play with.</li>
                        <li>Customize user defaults for new users, create, edit, delete existing users. </li>
                        <li>Create/Import your own database tables which also creates its own app for you to interact with.</li>
                        <li>Setup notifications that can alert users when a custom table or table import has been updated.</li>
                        <li>Add data charts to the custom tables and/or table imports.</li>
                        <li>Upload custom files and folders such as full web sites that can be hosted from this site.</li>
                        <li>Host web services.</li>
                        <li>Create and upload Site Plugins that can give the workspace extra functionality.</li>
                        <li>Track issues, user logins, site request, speeds, and enable the IP Listener which can make your site available to only certain IP addresses.</li>
                    </ul>
                    <div class="clear-space"></div>
                    <div align="center" class="pad-bottom-big">
                        <img alt="Network Log" src="Standard_Images/About Logos/networklog.jpg" class="screenshot-img" />
                        <div class="clear-space"></div>
                        <h3>Network Log</h3>
                    </div>
                    <div align="center" class="pad-bottom-big">
                        <img alt="Custom Tables" src="Standard_Images/About Logos/customtables.jpg" class="screenshot-img" />
                        <div class="clear-space"></div>
                        <h3>Custom Tables</h3>
                    </div>
                </div>
                <div class="clear"></div>
                <div class="about-section">
                    <h2>App Remote</h2>
                    <div class="clear-space-five"></div>
                    <ul style="list-style: disc; padding-left: 20px;">
                        <li>Control your workspace from your mobile device.</li>
                        <li>Use the App Remote on your mobile device to open up apps on the device and chat.</li>
                        <li>You do not have to be on the same network as your workspace. The App Remote works from anywhere as long as there is an internet connection.</li>
                    </ul>
                    <div class="clear-space"></div>
                    <div style="text-align: center">
                        <img alt="App Remote" src="Standard_Images/About Logos/appremote.jpg" class="screenshot-img" />
                    </div>
                </div>
                <div class="about-section">
                    <h2>Chat Client</h2>
                    <div class="clear-space-five"></div>
                    <ul style="list-style: disc; padding-left: 20px;">
                        <li>Communicate with other users that are apart of your group.</li>
                        <li>Add emoicons, images, url links, and even YouTube videos. These will be formated to view, click, and play in the chat session.</li>
                        <li>Set your current state to either Available, Away, Busy, or Offline.</li>
                    </ul>
                    <div class="clear-space"></div>
                    <div style="text-align: center">
                        <img alt="Chat Client" src="Standard_Images/About Logos/chatclient.jpg" class="screenshot-img" />
                    </div>
                </div>
                <div id="IWantThis" class="about-section"></div>
                <div class="about-section">
                    <h2>Wiki and Documentation</h2>
                    <div class="clear-space-five"></div>
                    <a href="https://github.com/jwdonnel/OpenWSE" target="_blank">Go to GitHub</a> to get updates and other documentation.
                    <div class="clear-space-two"></div>
                    An integration guide is available <a href="Integration.html" target="_blank">here.</a>
                    <div class="clear-space"></div>
                    OpenWSE is free for personal use as long as the copyright notices are intact. In order to remove the copyright, you must purchase a license. Download and run the OpenWSE_Installer.exe which will setup a website on your local machine using IIS. Once you buy the license, you can make any changes you want to the code. 
                    <div class="clear-space"></div>
                </div>
                <div class="about-section">
                    <h2>Frameworks and Technologies Used</h2>
                    <div class="clear-margin">
                        <div class="pad-left-big">
                            <ul style="list-style: disc;">
                                <li>Microsoft ASP.NET 4.5</li>
                                <li>JQuery <i>(<a href="http://www.jquery.com" target="_blank">www.jquery.com</a>)</i></li>
                                <li>JQuery UI <i>(<a href="http://jqueryui.com" target="_blank">www.jqueryui.com</a>)</i></li>
                                <li>TinyMCE - Javascript WYSIWYG editor</li>
                                <li>SharpZipLib - The Zip, GZip, BZip2 and Tar Implementation For .NET <i>(<a href="http://www.icsharpcode.net/opensource/sharpziplib"
                                    target="_blank">www.icsharpcode.net</a>)</i></li>
                                <li>Ace Editor <i>(<a href="http://ace.c9.io/" target="_blank">http://ace.c9.io/</a>)</i></li>
                                <li>Google APIs</li>
                            </ul>
                        </div>
                        <div class="clear-margin">
                            <b>This site works best in:</b>
                            <div class="clear-margin">
                                <div class="pad-left-big">
                                    <ul style="list-style: disc;">
                                        <li><a href="http://www.google.com/intl/en/chrome" target="_blank">Google Chrome</a></li>
                                        <li><a href="http://www.microsoft.com/windows/internet-explorer/default.aspx" target="_blank">Internet Explorer (Version 9 and up)</a></li>
                                        <li><a href="http://www.mozilla.com/firefox" target="_blank">Firefox (Version 3.6.27
                                and up)</a></li>
                                        <li><a href="https://www.apple.com/safari/" target="_blank">Safari</a></li>
                                    </ul>
                                </div>
                            </div>
                            <div class="clear-margin">
                                <small><b class="pad-right-sml">Note:</b>All other browsers not listed have not been
                tested and may not work properly with this website. Certain mobile devices may not
                be able to handle this website (Example: Android 2.3.x and below).</small>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <div class="clear-space">
            </div>
            <div align="center">
                <asp:Panel ID="pnlCreativeCommonLicense" runat="server">
                </asp:Panel>
            </div>
            <div class="clear" style="height: 75px;">
            </div>
            <div class="float-left pad-right-big">
                <img id="Img1" alt="Logo" src="~/Standard_Images/About Logos/openwse.png" runat="server" style="max-width: 175px;" />
            </div>
            <div class="float-left pad-left" style="max-width: 650px;">
                Icons on this website (Not including the OpenWSE and OnlineWSE logo) were downloaded from <a href="http://www.iconfinder.com" target="_blank">www.iconfinder.com</a> using the licensed terms of GPL: <a href="http://www.gnu.org/licenses/gpl.html"
                    target="_blank">GNU General Public License</a> Version 2 or later (the "GPL").
            </div>
            <div class="clear" style="height: 30px">
            </div>
            <b class="pad-right">Disclaimer:</b>By using this site, you agree that OpenWSE can save cookies to your computer or device. These cookies contain no information regarding your personal information.
            <div class="clear" style="height: 30px">
            </div>
            <div align="center" class="clear-margin">
                <div class="clear-space">
                </div>
                <a href="http://jquery.com" target="_blank" class='pad-left pad-right pad-bottom'>
                    <img alt="jquery" src="Standard_Images/About Logos/jquery.png" style="max-height: 75px; max-width: 100%;" /></a>
                <a href="http://www.asp.net" target="_blank" class='pad-left pad-right pad-bottom'>
                    <img alt="asp.net" src="Standard_Images/About Logos/aspnet.png" style="max-height: 75px; max-width: 100%;" /></a>
                <div class="clear-space"></div>
            </div>
            <div class="clear-space"></div>
        </div>
        <div id="termsofuse" style="display: none;">
            <h3 class="font-bold">Terms of Use</h3>
            <div class="clear-space"></div>
            <span class="font-bold pad-right-sml">Date:</span>August 16, 2015
                <div class="clear-space"></div>
            <div class="clear-space"></div>
            <div class="clear-space"></div>
            <div class="termsofuse-section">
                <div class="termsofus-section-title">1. Terms</div>
                <div class="termsofus-section-text">
                    By accessing this web site, you are agreeing to be bound by these web site Terms and Conditions of Use, all applicable laws and regulations, and agree that you are responsible for compliance with any applicable local laws. If you do not agree with any of these terms, you are prohibited from using or accessing this site. The materials contained in this web site are protected by applicable copyright and trade mark law.
                </div>
            </div>
            <div class="termsofuse-section">
                <div class="termsofus-section-title">2. Disclaimer</div>
                <div class="termsofus-section-text">
                    The materials on this site's web site are provided "as is". this site makes no warranties, expressed or implied, and hereby disclaims and negates all other warranties, including without limitation, implied warranties or conditions of merchantability, fitness for a particular purpose, or non-infringement of intellectual property or other violation of rights. Further, this site does not warrant or make any representations concerning the accuracy, likely results, or reliability of the use of the materials on its Internet web site or otherwise relating to such materials or on any sites linked to this site.
                </div>
            </div>
            <div class="termsofuse-section">
                <div class="termsofus-section-title">3. Limitations</div>
                <div class="termsofus-section-text">
                    Users signing in to this site must enter a valid username and password. Failure to do so can result in your IP being blocked by this site. If your IP is blocked due to this, or you feel that you are being blocked for other reasons, you can email John Donnelly at <a href="mailto:jwdonnel@gmail.com">jwdonnel@gmail.com</a>.
                </div>
            </div>
        </div>
        <div id="changeLog" style="display: none;">
            <asp:Literal ID="ltl_changeLog" runat="server"></asp:Literal>
        </div>
        <div id="helppages" style="display: none;">
            <div id="helppage-select-holder">
                <span class="pad-right font-bold">Pages</span>
                <select id="helppage-select" onchange="helpPageChange();">
                    <option value="Workspace">Workspace</option>
                    <option value="acctsettings">Account Settings</option>
                    <option value="appeditor">App Manager</option>
                    <option value="apppackages">App Packages</option>
                    <option value="customtables">Custom Tables</option>
                    <option value="dbimporter">Table Importer</option>
                    <option value="dbmanager">Database Manager</option>
                    <option value="dbviewer">Table Viewer</option>
                    <option value="fileeditor">File Editor</option>
                    <option value="grouporg">Group Organizer</option>
                    <option value="licensemanager">license Manager</option>
                    <option value="networklog">Network Log</option>
                    <option value="notifimanager">Notification Manager</option>
                    <option value="overlays">Overlay Manager</option>
                    <option value="servermanage">Site Settings</option>
                    <option value="siteplugins">Site Plugins</option>
                    <option value="startupscripts">Startup Scripts</option>
                    <option value="useraccounts">User Accounts</option>
                </select>
            </div>
            <div class="clear-space"></div>
            <div id="helpdiv_pageholder">
            </div>
        </div>
        <script type="text/javascript" src="//code.jquery.com/jquery-1.11.2.min.js"></script>
        <script type="text/javascript" src="//code.jquery.com/jquery-migrate-1.2.1.min.js"></script>
        <script type="text/javascript" src="//code.jquery.com/ui/1.11.4/jquery-ui.min.js"></script>
        <script type="text/javascript" src="Scripts/jquery/combined-scripts.min.js"></script>
        <script type="text/javascript" src="Scripts/SiteCalls/Full/openwse.js"></script>
        <script type="text/javascript">
            var availableHelpPages = "-";
            $(document).ready(function () {
                openWSE_Config.siteRootFolder = "<%=ResolveUrl("~/").Replace("/", "") %>";

                if (!inIframe()) {
                    var height = $("#always-visible").height();
                    $("#app_title_bg").css("margin-top", height);

                    $("#always-visible").show();
                }
                else {
                    $("#always-visible").hide();
                }

                var redirect = getParameterByName("redirect");
                if (redirect != null && redirect != "") {
                    $("#iframe-container-close-btn").find("a").attr("href", redirect);
                }

                if (availableHelpPages == "") {
                    $("#helppage-select-holder").remove();
                    $("#helpdiv_pageholder").html("<h3 align='center' class='pad-all'>There are no help pages available.</h3>");
                }
                else {
                    if (availableHelpPages != "-") {
                        $("#helppage-select").find("option").each(function () {
                            if (!containsHelpPage($(this).val())) {
                                $(this).remove();
                            }
                        });
                    }

                    helpPageChange();
                }

                $(window).resize();
            });

            function containsHelpPage(page) {
                var availableHelpPageArray = availableHelpPages.split(';');
                for (var i = 0; i < availableHelpPageArray.length; i++) {
                    if (availableHelpPageArray[i] == page) {
                        return true;
                    }
                }

                return false;
            }

            function inIframe() {
                try {
                    return window.self !== window.top;
                } catch (evt) {
                    return true;
                }
            }

            function helpPageChange() {
                $("#helpdiv_pageholder").removeClass("pad-top-big");
                $("#helpdiv_pageholder").removeClass("margin-top-big");

                var val = $("#helppage-select").val();
                if (val == "Workspace") {
                    openWSE.HelpMenuPageLoadWorkspace();
                    $("#helpdiv_pageholder").addClass("pad-top-big");
                    $("#helpdiv_pageholder").addClass("margin-top-big");
                }
                else {
                    var fullurl = openWSE.siteRoot() + "WebServices/SaveControls.asmx" + "/GetTotalHelpPages";
                    $("#helpdiv_pageholder").html("<h3 align='center' class='pad-all'>Loading. Please Wait...</h3>");
                    $.ajax({
                        url: fullurl,
                        type: "POST",
                        data: '{ "currentPage": "' + val + '" }',
                        contentType: "application/json; charset=utf-8",
                        success: function (data) {
                            var count = parseInt(data.d);
                            if (count > 0) {
                                totalHelpPages = count;
                                HelpMenuPageLoad(0, false);
                            }
                            else {
                                $("#helpdiv_pageholder").html("<h3 align='center' class='pad-all'>There are no help pages available for this webpage.</h3>");
                            }
                        },
                        error: function () {
                            $("#helpdiv_pageholder").html("<h3 align='center' class='pad-all' style='color: Red;'>There was an error retrieving the help pages. Please try again.</h3>");
                        }
                    });
                }
            }

            $(window).resize(function () {
                openWSE.ApplyMobileModeForMenuBar();

                $("#pnlLinkBtns").removeClass("mobile-pnlLinkBtns");
                $("#siteInfo").removeClass("mobile-siteInfo");
                $("#termsofuse").removeClass("mobile-termsofuse");
                $("#changeLog").removeClass("mobile-changeLog");

                if ($(".mobile-mode").length > 0) {
                    $("#pnlLinkBtns").addClass("mobile-pnlLinkBtns");
                    $("#siteInfo").addClass("mobile-siteInfo");
                    $("#termsofuse").addClass("mobile-termsofuse");
                    $("#changeLog").addClass("mobile-changeLog");
                }
            });
        </script>
    </form>
</body>
</html>
