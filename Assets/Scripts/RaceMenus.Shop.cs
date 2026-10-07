using System.Linq;
using UnityEngine;

namespace Racer
{
    // 0.89 campaign, round 1: the vehicle Shop, a page of the Garage (from the campaign screen and from the garage). Every
    // vehicle except the mower (the acorn reward) with the 0.76 turning preview, the 0.82 stat bars, its price and
    // Owned / Buy / Prize from "<event>" / Locked. Buying asks first. Upgrades (round 2) will be a row here per vehicle.
    public sealed partial class RaceMenus
    {
        string shopVehicle; bool shopFromCampaign;
        // starters, then the shop's vehicles by price, then the prizes
        static VehicleProfile[] ShopList => VehicleProfile.All.Where(p => !p.Reward)
            .OrderBy(p => CampaignData.Starters.Contains(p.Id) ? 0 : Campaign.Price(p.Id) > 0 ? 1 : 2).ThenBy(p => Campaign.Price(p.Id)).ToArray();
        VehicleProfile ShopProfile { get { var list = ShopList; var p = list.FirstOrDefault(v => v.Id == shopVehicle) ?? list[0]; shopVehicle = p.Id; return p; } }
        void BuildShopPreview()
        {
            var p = ShopProfile;
            title.text = "SHOP   ·   " + Campaign.Money(Campaign.Current.money);
            if (previewRoot) { previewRoot.SetActive(false); Destroy(previewRoot); }
            previewRoot = new GameObject("Shop display model"); previewRoot.layer = 31; previewRoot.transform.position = new(10000, 10000, 10000);
            VehicleVisual.Build(previewRoot.transform, p);
            var colors = flow.Save.Settings.bodyColors; int i = VehicleProfile.IndexOf(p.Id);
            if (colors != null && i >= 0 && i < colors.Length && colors[i] >= 0 && colors[i] < VehiclePaint.Colors.Length) VehiclePaint.Apply(previewRoot.transform, VehiclePaint.Colors[colors[i]]);
            FramePreview(p);
        }
        string ShopStatus(VehicleProfile p, out bool canBuy)
        {
            canBuy = Campaign.CanBuy(p, out string why);
            if (Campaign.Owns(p.Id)) return "OWNED" + (CampaignData.Starters.Contains(p.Id) ? "  (starter)" : "");
            if (Campaign.Testing) return why;
            if (canBuy) return "Price: " + Campaign.Money(Campaign.Price(p.Id));
            if (Campaign.Price(p.Id) > 0) return Campaign.ChapterOpen(Campaign.PriceChapter(p.Id)) ? "Price: " + Campaign.Money(Campaign.Price(p.Id)) + "  ·  not enough money yet" : why;
            var prize = Campaign.PrizeEvent(p.Id); if (prize != null) return $"PRIZE from \"{prize.Name}\" (chapter {prize.Chapter}): win it";
            return "PRIZE: " + Campaign.HowToGet(p);
        }
        void RenderShop()
        {
            var p = ShopProfile; var list = ShopList; int at = System.Array.IndexOf(list, p);
            ClearCore("SHOP   ·   " + Campaign.Money(Campaign.Current.money), $"{p.Name} / {p.Class}\n{p.Description}\n{ShopStatus(p, out bool canBuy)}");
            details.fontSize = 18; details.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 84;
            swatchRow.gameObject.SetActive(false); preview.gameObject.SetActive(true);
            void StepShop(int d) { shopVehicle = list[(at + d + list.Length) % list.Length].Id; flow.Click(); Show(); }
            Step(0, "shop-vehicle", $"{p.Name}  ({at + 1} of {list.Length})", StepShop);
            buttons[0].GetComponentInChildren<UnityEngine.UI.Text>(true).alignment = TextAnchor.MiddleCenter;
            int price = Campaign.Price(p.Id);
            Row(1, "buy", Campaign.Owns(p.Id) ? "Owned" : price > 0 ? "BUY   ·   " + Campaign.Money(price) : "Not for sale", () =>
                Confirm("BUY THE " + p.Name.ToUpperInvariant() + "?", $"{Campaign.Money(price)} of your {Campaign.Money(Campaign.Current.money)}. It is yours in the campaign, Race and Free Roam.", () =>
                {
                    if (Campaign.Buy(p)) flow.Notify("Bought: " + p.Name + "  ·  choose it in the Garage", 5);
                    else { Campaign.CanBuy(p, out string why); flow.Notify(Campaign.Error ?? why, 5); }
                }, "BUY"));
            buttons[1].interactable = canBuy;
            Row(2, "shop-back", "Back", ShopBack);
            LayoutGarageBody(); ShowGarageStats(p);
            details.transform.SetSiblingIndex(0); buttons[0].transform.SetSiblingIndex(1); statBlock.SetSiblingIndex(2); buttons[1].transform.SetSiblingIndex(3); buttons[2].transform.SetSiblingIndex(4);
        }
        void ShopBack()
        {
            if (pages.Count > 0) { BackPage(); return; }
            page = ""; bool toCampaign = shopFromCampaign; shopFromCampaign = false; flow.PopMenu();
            if (toCampaign) OpenCampaign();
        }
    }
}
