var rackMenu = null;
var rackItemsMenu = null;

var rackLockOption = null;
var rackRemoveOption = null;

var rackMenus = [];
var rackItemTaking = false;

var displayRackText = false;
var lastMenuIndex = 0;

function numberWithCommas(x) {
	return x.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
}

function hideMenus() {
	for (var i = 0; i < rackMenus.length; i++) rackMenus[i].Visible = false;
}

API.onResourceStart.connect(function() {
	rackMenu = API.createMenu(" ", "~b~Gun Rack: Main Menu", 0, 0, 6);
	API.setMenuBannerSprite(rackMenu, "shopui_title_gr_gunmod", "shopui_title_gr_gunmod");
	rackMenus.push(rackMenu);

	rackItemsMenu = API.createMenu(" ", "", 0, 0, 6);
	API.setMenuBannerSprite(rackItemsMenu, "shopui_title_gr_gunmod", "shopui_title_gr_gunmod");
	rackMenus.push(rackItemsMenu);

	rackItemsMenu.ParentMenu = rackMenu;

	var temp_item = API.createMenuItem("Put Gun", "Put your current weapon to the gun rack.");
	rackMenu.AddItem(temp_item);
	rackMenu.BindMenuToItem(rackItemsMenu, temp_item);

	temp_item = API.createMenuItem("Take Gun", "Take a gun from the gun rack.");
	rackMenu.AddItem(temp_item);
	rackMenu.BindMenuToItem(rackItemsMenu, temp_item);
	
	rackLockOption = API.createCheckboxItem("Lock", "Lock/unlock the gun rack.", false);

	rackLockOption.CheckboxEvent.connect(function(sender, checked) {
		rackLockOption.Checked = !checked;
		API.triggerServerEvent("Rack_SetLock", checked);
	});

	rackMenu.AddItem(rackLockOption);

	rackRemoveOption = API.createColoredItem("Remove", "Remove the gun rack.", "#e03232", "#ffffff");
	rackMenu.AddItem(rackRemoveOption);

	rackMenu.OnItemSelect.connect(function(sender, item, index) {
		switch (index)
		{
			case 0:
				rackItemTaking = false;
				rackItemsMenu.RefreshIndex();

				API.setMenuSubtitle(rackItemsMenu, "~b~Gun Rack: Put Gun");
			break;

			case 1:
				rackItemTaking = true;
				rackItemsMenu.RefreshIndex();

				API.setMenuSubtitle(rackItemsMenu, "~b~Gun Rack: Take Gun");
			break;

			case 3:
				API.triggerServerEvent("Rack_Remove");
				rackMenu.Visible = false;
			break;
		}
	});

	rackItemsMenu.OnItemSelect.connect(function(sender, item, index) {
		lastMenuIndex = index;

		if (rackItemTaking) {
			API.triggerServerEvent("Rack_TakeSelected", index);
		} else {
			API.triggerServerEvent("Rack_PutToSelected", index);
		}
	});
});

API.onServerEventTrigger.connect(function (name, args) {
	switch (name)
	{
		case "SetRackState":
			displayRackText = args[0];

			if (!displayRackText) hideMenus();
		break;

		case "SetLockState":
			rackLockOption.Checked = args[0];
		break;

		case "Rack_ShowMenu":
			rackItemsMenu.Clear();

			var data = JSON.parse(args[0]);
			rackLockOption.Checked = data.Locked;
			
			for (var i = 0; i < data.Weapons.length; i++)
			{
				var temp_gun_item = null;

				if (data.Weapons[i] == null) {
					temp_gun_item = API.createMenuItem((i + 1) + ". Empty Slot", "");
				} else {
					temp_gun_item = API.createMenuItem((i + 1) + ". " + data.Weapons[i].WeaponName, "");
					temp_gun_item.SetRightLabel("Ammo: " + numberWithCommas(data.Weapons[i].Ammo));
				}

				rackItemsMenu.AddItem(temp_gun_item);
			}

			rackMenu.RefreshIndex();
			rackItemsMenu.RefreshIndex();

			rackMenu.Visible = true;
		break;

		case "Rack_UpdateWeapons":
			rackItemsMenu.Clear();

			var data = JSON.parse(args[0]);
			for (var i = 0; i < data.Weapons.length; i++)
			{
				var temp_gun_item = null;

				if (data.Weapons[i] == null) {
					temp_gun_item = API.createMenuItem((i + 1) + ". Empty Slot", "");
				} else {
					temp_gun_item = API.createMenuItem((i + 1) + ". " + data.Weapons[i].WeaponName, "");
					temp_gun_item.SetRightLabel("Ammo: " + numberWithCommas(data.Weapons[i].Ammo));
				}

				rackItemsMenu.AddItem(temp_gun_item);
			}

			rackItemsMenu.CurrentSelection = lastMenuIndex;
		break;
	}
});

API.onKeyDown.connect(function(e, key) {
	if (key.KeyCode == Keys.E)
	{
		if (displayRackText)
		{
			var visibleItems = rackMenus.filter(function(menu) {
				return (menu.Visible);
			});

			if (visibleItems.length > 0) {
				hideMenus();
			} else {
				API.triggerServerEvent("Rack_Interact");
			}
		}
	}
});

API.onUpdate.connect(function () {
	if (displayRackText) API.displaySubtitle("Press ~y~E ~w~to interact with the gun rack.", 100);
	for (var i = 0; i < rackMenus.length; i++) API.drawMenu(rackMenus[i]);
});