function Glass::init(%context) {
	if(!isObject(Glass)) {
		new ScriptObject(Glass) {
			version = "2.0.0-beta.4";
			address = "test.blocklandglass.com";
			netAddress = "test.blocklandglass.com";
			enableCLI = true;
		};

		if($Pref::Player::NetName $= "Jincux") {
			Glass.dev = true;
		}
	}

	$Glass::Debug = true;

	if(%context $= "client") {
		Glass::execClient();
	} else {
		Glass::execServer();
	}
}

function Glass::debug(%text) {
	if(Glass.dev) {
		echo(%text);
	}
}

function JettisonObject::get(%this, %key) {
	return %this.value[%key];
}
