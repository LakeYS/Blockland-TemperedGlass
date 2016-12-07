function GlassServers::init() {
  if(GlassSettings.get("Servers::EnableFavorites") == 0)
    return;
  GlassServerPreview_Favorite.setVisible(true);
  GlassFavoriteServers::changeGui();
  GlassLoading::changeGui();

  new ScriptObject(GlassFavoriteServers);
  %this = GlassFavoriteServers;

  %favs = GlassSettings.get("Servers::Favorites");
  %this.favorites = 0;
  for(%i = 0; %i < getFieldCount(%favs); %i++) {
    %this.favorite[%this.favorites] = getField(%favs, %i);
    %this.favorites++;
  }

  GlassFavoriteServers.scanServers();
}

//====================================
// Favorite Servers
//====================================

function GlassFavoriteServers::changeGui() {
  if(!isObject(GlassFavoriteServerSwatch)) {
    exec("./gui/elements/GlassHighlightSwatch.cs");
    exec("./gui/GlassFavoriteServer.gui");
  }

  MainMenuButtonsGui.add(GlassFavoriteServerSwatch);
}

function GlassFavoriteServers::toggleFavorite(%this, %ip) {
  if(%ip $= "")
    return;

  %favs = GlassSettings.get("Servers::Favorites");

  for(%i = 0; %i < getFieldCount(%favs); %i++) {
    if(getField(%favs, %i) $= %ip) {
	  GlassSettings.update("Servers::Favorites", removeField(%favs, %i));
    glassMessageBoxOk("Favorite Removed", "This server has been removed from your favorites!");
	  GlassServerPreview_Favorite.mColor = "46 204 113 220";
	  GlassServerPreview_Favorite.setText("Add Favorite");
	  GlassServers::init();
      return;
    }
  }

  glassMessageBoxOk("Favorite Added", "This server has been added to your favorites!");
  GlassSettings.update("Servers::Favorites", trim(%favs TAB %ip));
  GlassServerPreview_Favorite.mColor = "231 76 60 220";
  GlassServerPreview_Favorite.setText("Remove Favorite");
  GlassServers::init();
}


function GlassFavoriteServers::buildList(%this) {
  for(%i = 0; %i < GlassFavoriteServerSwatch.getCount(); %i++) {
    %obj = GlassFavoriteServerSwatch.getObject(%i);
	  %name = %obj.getName();

    if(%name !$= "GlassFavoriteServerGui_Text" && %name !$= "GlassFavoriteServerGui_Tutorial" && %name !$= "GlassFavoriteServerGui_NoServers") {
      %obj.deleteAll();
      %obj.delete();
      %i--;
    }
  }

  for(%i = 1; %i <= %this.onlineFavoriteCount; %i++) {
    %swatch = new GuiSwatchCtrl("GlassFavoriteServerGui_Swatch" @ %i) {
      profile = "GuiDefaultProfile";
      horizSizing = "right";
      vertSizing = "bottom";
      position = "10 35";
      extent = "270 47";
      minExtent = "8 2";
      enabled = "1";
      visible = "1";
      clipToParent = "1";
      color = "220 220 220 255";
    };

    %swatch.text = new GuiMLTextCtrl() {
      profile = "GuiMLTextProfile";
      horizSizing = "right";
      vertSizing = "bottom";
      position = "10 10";
      extent = "250 27";
      minExtent = "8 2";
      enabled = "1";
      visible = "1";
      clipToParent = "1";
      lineSpacing = "2";
      allowColorChars = "0";
      maxChars = "-1";
      maxBitmapHeight = "-1";
      selectable = "1";
      autoResize = "1";
    };

    %swatch.add(%swatch.text);

    %swatch.text.setText("<font:verdana bold:15>" @ %this.favorite[%i] @ "<br><just:center><font:verdana:13>Loading...");

    GlassHighlightSwatch::addToSwatch(%swatch, "0 0 0 0", "GlassFavoriteServers::interact");

    GlassFavoriteServerSwatch.add(%swatch);

    if(%placeBelow)
      %swatch.placeBelow(%placeBelow, 5);

    %placeBelow = %swatch;

  	%server = GlassFavoriteServers.onlineFavorite[%i];

  	%password = getField(%server, 3);
  	GlassFavoriteServers.renderServer((%passworded ? "passworded" : "online"), %i, getField(%server, 2), getField(%server, 4), getField(%server, 5), getField(%server, 6), getField(%server, 0) @ getField(%server, 1));
  }

  if(%this.favorites $= "" || %this.favorites == 0) {
    GlassFavoriteServerSwatch.extent = "290 60";
  	GlassFavoriteServerGui_Tutorial.setVisible(true);
  	GlassFavoriteServerGui_NoServers.setVisible(false);
  } else if(%this.onlineFavoriteCount == 0) {
	  GlassFavoriteServerGui_NoServers.setVisible(true);
	  GlassFavoriteServerGui_Tutorial.setVisible(false);
  } else {
  	GlassFavoriteServerGui_Tutorial.setVisible(false);
  	GlassFavoriteServerGui_NoServers.setVisible(false);
  }
  GlassFavoriteServerSwatch.verticalMatchChildren(24, 10);
  GlassFavoriteServerSwatch.position = vectorSub(MainMenuButtonsGui.extent, GlassFavoriteServerSwatch.extent);
}

function GlassFavoriteServers::renderServer(%this, %status, %id, %title, %players, %maxPlayers, %map, %addr) {
  %swatch = "GlassFavoriteServerGui_Swatch" @ %id;
  //if(%swatch.text $= "")
    %swatch.text = %swatch.getObject(0);


  %swatch.server = new ScriptObject() {
    name = trim(%title);
    pass = (%status $= "passworded" ? "Yes" : "No");
    currPlayers = %players;
    maxPlayers = %maxPlayers;

    ip = %addr;

    offline = %status $= "offline";
  };

  switch$(%status) {
    case "online":
      %swatch.color = "131 195 243 255";
      %swatch.text.setText("<font:verdana bold:15>" @ %title @ "<br><font:verdana:13>" @ %players @ "/" @ %maxPlayers @ " Players<just:right>" @ %map);

    case "passworded":
      %swatch.color = "235 153 80 255";
      %swatch.text.setText("<font:verdana bold:15>" @ trim(%title) @ " <font:verdana:13>(Passworded)<br>" @ %players @ "/" @ %maxPlayers @ " Players<just:right>" @ %map);

    case "offline":
      %swatch.color = "220 220 220 255";
      %swatch.text.setText("<font:verdana bold:15>" @ %title @ "<br><font:verdana:13>Offline");
  }

  %swatch.ocolor = %swatch.color;
  %swatch.hcolor = %swatch.color;
  %swatch.pushToBack(%swatch.glassHighlight);
}

function GlassFavoriteServers::scanServers() {
	if(!isObject(GlassFavoriteServers))
	  return;

  connectToUrl("master2.blockland.us", "GET", "", "GlassFavoriteServersTCP");
}

function GlassFavoriteServers::interact(%swatch) {
  %server = %swatch.server;
  if(!%server.offline)
    GlassServerPreviewGui.open(%server);
  else
    glassMessageBoxOk("Offline", %server.name @ " is currently offline!");
}

function GlassFavoriteServersTCP::handleText(%this, %text) {
  %this.buffer = %this.buffer NL %text;
}

function GlassFavoriteServersTCP::onDone(%this, %err) {
  if(%err) {
    GlassFavoriteServers.renderError(%err);
  } else {
	%onlineCount = 0;
	for(%i = 0; %i < getLineCount(%this.buffer); %i++) {
      %line = getLine(%this.buffer, %i);
	  %serverIP = trim(getField(%line, 0) @ ":" @ getField(%line, 1));

	  for(%j = 0; %j < GlassFavoriteServers.favorites; %j++) {
	    %fav = GlassFavoriteServers.favorite[%j];
		if(%fav $= %serverIP) {
		  %passworded = getField(%line, 2);
		  %serverName = getField(%line, 4);
		  %players = getField(%line, 5);
          %maxPlayers = getField(%line, 6);
          %map = getField(%line, 7);
		  GlassFavoriteServers.onlineFavorite[%onlineCount++] = %serverIP TAB %serverPort TAB %serverName TAB %passworded TAB %players TAB %maxPlayers TAB %map;
		}
	  }
	}
	GlassFavoriteServers.onlineFavoriteCount = %onlineCount;
	GlassFavoriteServers.buildList();
  }
}


//====================================
// LoadingGui
//====================================

function GlassLoading::changeGui() {
  if(LoadingGui.isGlass) {
    return;
  }

  %loadingGui = LoadingGui.getId();

  exec("./gui/LoadingGui.gui");

  %loadingGui.deleteAll();
  %loadingGui.delete();

  LoadingGui.isGlass = true;
}

function GlassLoadingGui::updateWindowTitle(%this) {
  %npl = NPL_Window.getValue();
  // %name = trim(getSubStr(%npl, strPos(%npl, "-") + 2, strLen(%npl)));

  // if(%name !$= "")
    // %text = "Joining \"" @ %name @ "\"";
  // else
    // %text = "Joining Server";

  // %this.setText(%text);
  %this.setText(%npl);
}

function GlassLoadingGui::onWake(%this) {
  GlassLoadingGui_Image.setBitmap("Add-Ons/System_BlocklandGlass/image/gui/noImage.png");
  GlassServerPreview::getServerBuild(ServerConnection.getAddress(), GlassLoadingGui_Image);
}

//====================================
// ServerPreview
//====================================

function GlassServerPreviewGui::open(%this, %server) {
  if(%server $= "") {
    %server = ServerInfoGroup.getObject(JS_ServerList.getSelectedID());
  }
  %this.server = %server;
  if(joinServerGui.isAwake()) {
    canvas.popDialog(joinServerGui);
    %this.wakeServerGui = true;
  }
  canvas.pushDialog(GlassServerPreviewGui);
}

function GlassServerPreviewGui::close(%this) {
  canvas.popDialog(GlassServerPreviewGui);
  if(%this.wakeServerGui) {
    canvas.pushDialog(joinServerGui);
    %this.wakeServerGui = false;
  }
}

function getServerFromIP(%ip) {
  if(!isObject(ServerInfoGroup))
	  JoinServerGui.queryWebMaster();
  for(%i=0; %i < ServerInfoGroup.getCount(); %i++) {
	%search = ServerInfoGroup.getObject(%i);
	if(%search.ip $= %ip)
		return %search;
  }
  return -1;
}

function GlassServerPreviewGui::onWake(%this) {
  GlassServerPreviewWindowGui.forceCenter();

  %server = %this.server;

  if(!isObject(%server))
	  return;

  if(%server.pass !$= "No") {
    %img = "<bitmap:Add-Ons/System_BlocklandGlass/image/icon/lock>";
    GlassServerPreview_Connect.mColor = "235 153 80 220";
  } else if(%server.currPlayers >= %server.maxPlayers) {
    %img = "<bitmap:Add-Ons/System_BlocklandGlass/image/icon/group_error>";
    GlassServerPreview_Connect.mColor = "237 118 105 220";
  } else {
    GlassServerPreview_Connect.mColor = "84 217 140 220";
  }

  GlassServerPreview_Name.setText("<font:verdana bold:18>" @ trim(%server.name) SPC %img @ "<br><font:verdana:15>" @ %server.currPlayers @ "/" @ %server.maxPlayers SPC "Players");
  GlassServerPreview_Preview.setBitmap("Add-Ons/System_BlocklandGlass/image/gui/noImage.png");
  GlassServerPreview_Playerlist.clear();
  GlassServerPreview::getServerInfo(%server.ip);
  GlassServerPreviewWindowGui.openServerIP = %server.ip;
  GlassServerPreviewWindowGui.openServerName = %server.name;

  GlassServerPreview::getServerBuild(%server.ip, GlassServerPreview_Preview);

  if(GlassSettings.get("Servers::EnableFavorites")) {
    for(%i = 0; %i < GlassFavoriteServers.favorites; %i++) {
      %fav = GlassFavoriteServers.favorite[%i];

	  if(%fav $= %server.ip) {
	  	GlassServerPreview_Favorite.mColor = "231 76 60 220";
	  	GlassServerPreview_Favorite.setText("Remove Favorite");
	  	break;
	  } else {
	  	GlassServerPreview_Favorite.mColor = "46 204 113 220";
	  	GlassServerPreview_Favorite.setText("Add Favorite");
	  }
    }
  }
}

function GlassServerPreview::connectToServer() {
  %server = GlassServerPreviewWindowGui.openServerIP;
  if(%server $= "")
	return;
  
  ConnectToServer(%server, "", "1", "1");
}
	

function GlassServerPreview::getServerBuild(%addr, %obj) {
  %addr = strReplace(%addr, ".", "-");
  %addr = strReplace(%addr, ":", "_");
  %url = "http://image.blockland.us/detail/" @ %addr @ ".jpg";
  %method = "GET";
  %downloadPath = "config/client/BLG/ServerPreview.jpg";
  %className = "GlassServerPreviewTCP";

  %tcp = connectToUrl(%url, %method, %downloadPath, %className);
  %tcp.bitmap = %obj;
}

function GlassServerPreviewTCP::onDone(%this, %error) {
  if(%error) {
    echo("ERROR:" SPC %error);
  }

  %this.bitmap.setBitmap("config/client/BLG/ServerPreview.jpg");
}

function GlassServerPreview::getServerInfo(%addr) {
  %idx = strpos(%addr, ":");

  %ip = getSubStr(%addr, 0, %idx);
  %port = getSubStr(%addr, %idx+1, strlen(%addr));

  %url = "http://api.blocklandglass.com/api/3/serverStats.php?ip=" @ urlEnc(%ip) @ "&port=" @ urlEnc(%port);
  %method = "GET";
  %downloadPath = "";
  %className = "GlassServerPreviewPlayerTCP";

  %tcp = connectToUrl(%url, %method, %downloadPath, %className);
}

function GlassServerPreviewPlayerTCP::handleText(%this, %text) {
  %this.buffer = %this.buffer NL %text;
}

function GlassServerPreviewPlayerTCP::onDone(%this, %error) {
  if(%error) {
    echo("ERROR:" SPC %error);
  } else {
    %err = jettisonParse(%this.buffer);
    if(%err) {
      //parse error, $JSON::Error
      return;
    }

    %result = $JSON::Value;

    if(%result.status $= "error") {
      GlassServerPreview_Playerlist.clear();
      GlassServerPreview_noGlass.setVisible(true);
    } else {
      GlassServerPreview_noGlass.setVisible(false);

      %playerCount = %result.Clients.length;

      if(%result.clients.value[0].name $= "") // empty serv
        return;

      for(%i=0; %i < %playerCount; %i++) {
        %cl = %result.clients.value[%i];

        if(%cl.status $= "")
          %cl.status = "-";

        GlassServerPreview_Playerlist.addRow(%cl.blid, "  " @ %cl.status TAB %cl.name TAB %cl.blid);
      }
    }
  }
}

function joinServerGui::preview(%this) {
  if(JS_ServerList.getSelectedID() == -1)
	  return;

  GlassServerPreviewGui.open();
}

function clientCmdGlass_setLoadingBackground(%url, %filetype) {
  if(GlassSettings.get("Servers::LoadingImages") == 0)
	  return;

  if(!LoadingGUI.isAwake())
	return;

  if(LoadingGUI.lastDownload + 2 > $Sim::Time)
	  return;

  LoadingGUI.lastDownload = $Sim::Time;

  if(%fileType !$= "jpg" && %fileType !$= "png" && %fileType !$= "jpeg") {
	echo("Cannot download loading screen background as it does not have a legal file type.");
	return;
  }

  %method = "GET";
  %downloadPath = "config/client/BLG/loadingBackground." @ %fileType;
  %className = "GlassServerBackgroundTCP";

  %tcp = connectToUrl(%url, %method, %downloadPath, %className);
  %tcp.fileType = %fileType;
}

function GlassServerBackgroundTCP::onBinChunk(%this, %chunk) {
	if(%chunk >= 2000000) {
		warn("Error - GlassServerBackgroundTCP file is greater than 2mb, stopping download.");
		%this.disconnect();
	}
}

function GlassServerBackgroundTCP::onDone(%this, %error) {
  if(%error) {
    echo("GlassServerBackgroundTCP error:" SPC %error);
	return;
  }
  LOAD_MapPicture.setBitmap("base/client/ui/loadingBG");
  LOAD_MapPicture.setBitmap("config/client/BLG/loadingBackground." @ %this.fileType);
}

package GlassServers {
  function joinServerGui::onWake(%this) {
  	if(!%this.initializedGlass) {
  	  %this.initializedGlass = 1;
  	  joinServerGui.clear();
  	  joinServerGui.add(GlassJS_window);
  	  GlassJS_window.setName("JS_window");
  	}
  	parent::onWake(%this);
  }

  function NPL_List::addRow(%this, %id, %val) {
    GlassLoadingGui_UserList.addRow(%id, %val);
    return parent::addRow(%this, %id, %val);
  }

  function NPL_List::clear(%this) {
    GlassLoadingGui_UserList.clear();
    parent::clear(%this);
  }

  function MainMenuButtonsGui::onWake(%this) {
	if(isObject(GlassFavoriteServers))
      GlassFavoriteServers.scanServers();

    if(isFunction(%this, onWake))
      parent::onWake(%this);
  }

  function LoadingGui::onWake(%this) {
	LOAD_MapPicture.setBitmap("base/client/ui/loadingBG");
    if(isFunction(LoadingGui, onWake))
      parent::onWake(%this);

    LoadingGui.pushToBack(GlassLoadingGui);
  }

  function NewPlayerListGui::UpdateWindowTitle(%gui) {
    parent::UpdateWindowTitle(%gui);

    GlassLoadingGui.updateWindowTitle();
  }
};
activatePackage(GlassServers);
