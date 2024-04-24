using System;
using System.Collections;
using System.Collections.Generic;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Newtonsoft.Json;
using System.IO;

namespace ConsoleRPG
{
    internal class Program
    {
        private static string dataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "setting.json");
        private static Setting setting;

        class Setting
        {
            public string[] gameItems = { "수련자 갑옷", "무쇠갑옷", "스파르타의 갑옷", "낡은 검", "청동 도끼", "스파르타의 창" };
            public List<string> myItems = new List<string>();
            public List<string> equippedItems = new List<string>(); // 장착된 아이템 리스트
            public float armor = 5;
            public float attack = 10;
            public float health = 100;
            public int level = 1;
            public float gold = 1000f;
            public int price;
            public Random random = new Random();
            public int dungeonclearCnt = 0;
        }

        static void Main(string[] args)
        {
            LoadData();
            StartScene();
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);
            Console.CancelKeyPress += new ConsoleCancelEventHandler(Console_CancelKeyPress);

            
        }

        static void SaveData()
        {
            try
            {
                string json = JsonConvert.SerializeObject(setting, Formatting.Indented);
                File.WriteAllText("setting.json", json);
                Console.WriteLine("데이터가 성공적으로 저장되었습니다.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("데이터 저장 중 오류가 발생했습니다:");
                Console.WriteLine(ex.Message);
            }
        }

        static void LoadData()
        {
            if (File.Exists(dataPath))
            {
                string json = File.ReadAllText(dataPath);
                setting = JsonConvert.DeserializeObject<Setting>(json);
            }
            else
            {
                setting = new Setting();
            }
        }
        static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            SaveData();
        }

        static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            SaveData();
        }


        static void StartScene()
        {
            {
                Console.WriteLine("스파르타 마을에 오신 여러분 환영합니다.");
                Console.WriteLine("이곳에서 던전으로 들어가기전 활동을 할 수 있습니다.");
                Console.WriteLine("1. 상태 보기");
                Console.WriteLine("2. 인벤토리");
                Console.WriteLine("3. 상점");
                Console.WriteLine("4. 던전입장");
                Console.WriteLine("5. 휴식하기");
                Console.WriteLine("6. 데이터 저장하기"); 
                Console.WriteLine("0. 게임 종료"); 

                int selcetNum = int.Parse(Console.ReadLine());

                switch (selcetNum)
                {
                    case 1:
                        ShowStatus();
                        break;
                    case 2:
                        if (setting.myItems.Count == 0)
                        {
                            NoneInventory();
                        }
                        else
                        {
                            Inventory();
                        }
                        break;
                    case 3:
                        Store();
                        break;
                    case 4:
                        Dungeon();
                        break;
                    case 5:
                        TakeRest();
                        break;
                    case 6:
                        SaveData(); // 데이터 저장하기 메뉴
                        Console.Clear();
                        Console.WriteLine("데이터가 저장되었습니다.");
                        StartScene();
                        break;
                    case 0:
                        SaveData(); // 데이터 저장하기 메뉴
                        Environment.Exit(0);
                        break;
                    default:
                        Console.WriteLine("다시누르세요");
                        StartScene();
                        break;
                }
            }

            static void ShowStatus()
            {
                if(setting.health <= 0) setting.health = 0;
                string levelUI = setting.level.ToString("D2");
                Console.Clear();
                Console.WriteLine("[정 보]");
                Console.WriteLine();
                Console.WriteLine($"Lv. {levelUI}");
                Console.WriteLine("Chad ( 전사 )");
                if (setting.equippedItems.Count == 0)
                {
                    Console.WriteLine($"공격력 : {setting.attack}");
                    Console.WriteLine($"방어력 : {setting.armor}");
                }
                else
                {
                    float totalPower = setting.attack;
                    float totalArmor = setting.armor;

                    foreach (string item in setting.equippedItems)
                    {
                        totalPower += GetAttackPower(item);
                        totalArmor += GetArmor(item);
                    }

                    Console.WriteLine($"공격력 : {totalPower} (+{totalPower - setting.attack})");
                    Console.WriteLine($"방어력 : {totalArmor} (+{totalArmor - setting.armor})");
                }
                Console.WriteLine($"체 력 : {setting.health}");
                Console.WriteLine($"Gold :{setting.gold}G");
                Console.WriteLine();
                Console.WriteLine("0. 나가기");
                Console.WriteLine("원하시는 행동을 입력해주세요.");
                string answer = Console.ReadLine();
                if (answer.ToLower() == "0")
                {
                    StartScene();
                }
                else
                {
                    Console.Clear();
                    ShowStatus();
                }
            }

            static void NoneInventory()
            {
                Console.Clear();
                Console.WriteLine();
                Console.WriteLine("[아이템 목록]");
                Console.WriteLine();
                Console.WriteLine("0. 나가기");
                Console.WriteLine("원하시는 행동을 입력해주세요.");
                string answer = Console.ReadLine();
                if (answer.ToLower() == "0")
                {
                    StartScene();
                }
            }

            static void Inventory()
            {
                Console.Clear();
                Console.WriteLine();
                Console.WriteLine("[아이템 목록]");
                for (int i = 0; i < setting.myItems.Count; i++)
                {
                    string item = setting.myItems[i];
                    string itemInfo = GetItemInfo(item);
                    string equippedIndicator = IsItemEquipped(item) ? "[E]" : "";
                    Console.WriteLine($"{i + 1}. {equippedIndicator} {item} | {itemInfo}");
                }
                Console.WriteLine();
                Console.WriteLine("1. 장착 관리");
                Console.WriteLine("2. 나가기");
                Console.WriteLine("원하시는 행동을 입력해주세요.");

                string answer = Console.ReadLine();
                if (answer.ToLower() == "2")
                {
                    StartScene();
                }
                else if (answer.ToLower() == "1")
                {
                    ManageInventory();
                }
                else
                {
                    Console.WriteLine("다시누르세요");
                    Inventory();
                }
            }

            static void ManageInventory()
            {
                Console.Clear();
                string wear = "[E]";
                Console.WriteLine("[아이템 목록]");
                for (int i = 0; i < setting.myItems.Count; i++)
                {
                    string item = setting.myItems[i];
                    string itemInfo = GetItemInfo(item);
                    string equippedIndicator = IsItemEquipped(item) ? wear : "";
                    Console.WriteLine($"{i + 1}. {item} | {itemInfo} {equippedIndicator}");
                }
                Console.WriteLine();
                Console.WriteLine("0. 나가기");
                Console.WriteLine("원하시는 행동을 입력해주세요.");

                string answer = Console.ReadLine();
                if (answer.ToLower() == "0")
                {
                    StartScene();
                }
                else if (int.TryParse(answer, out int selectedNum) && selectedNum > 0 && selectedNum <= setting.myItems.Count)
                {
                    string selectedItem = setting.myItems[selectedNum - 1];
                    ToggleEquipStatus(selectedItem);
                    Console.WriteLine(IsItemEquipped(selectedItem) ?
                        $"{wear}{selectedItem} | {GetItemInfo(selectedItem)}" :
                        $"{selectedItem} | {GetItemInfo(selectedItem)}");
                    ManageInventory();
                    Console.WriteLine("0. 나가기");
                    Console.WriteLine("원하시는 행동을 입력해주세요.");
                    answer = Console.ReadLine();
                    if (answer.ToLower() == "0")
                    {
                        Console.Clear();
                        StartScene();
                    }
                }
                else
                {
                    Console.Clear();
                    Console.WriteLine("다시누르세요");
                    ManageInventory();
                }
            }

            static void Store()
            {
                Console.Clear();
                Console.WriteLine();
                Console.WriteLine("필요한 아이템을 얻을 수 있는 상점입니다.");
                Console.WriteLine();
                Console.WriteLine("[보유골드]");
                Console.WriteLine($"{setting.gold}G");
                Console.WriteLine();
                Console.WriteLine("[아이템 목록]");
                for (int i = 0; i < setting.gameItems.Length; i++)
                {
                    string item = setting.gameItems[i];
                    string itemInfo = GetItemInfo(item);
                    int itemPrice = GetItemPrice(item);
                    Console.WriteLine($"{item} | {itemInfo} | {itemPrice} G");
                }
                Console.WriteLine();
                Console.WriteLine("1. 아이템 구매");
                Console.WriteLine("2. 아이템 판매");
                Console.WriteLine("0. 나가기");
                Console.WriteLine("원하시는 행동을 입력해주세요.");

                string answer = Console.ReadLine();
                if (answer.ToLower() == "0")
                {
                    Console.Clear();
                    StartScene();
                }
                else if (answer.ToLower() == "1")
                {
                    Console.Clear();
                    BuyItem();
                }
                else if (answer.ToLower() == "2")
                {
                    Console.Clear();
                    SellItem();
                }
                else
                {
                    Console.Clear();
                    Console.WriteLine("다시누르세요");
                    Store();
                }
            }
            static void BuyItem()
            {
                Console.WriteLine();
                Console.WriteLine("필요한 아이템을 얻을 수 있는 상점입니다.");
                Console.WriteLine("구매할 아이템 번호를 입력하세요:");
                Console.WriteLine();
                Console.WriteLine("[보유골드]");
                Console.WriteLine($"{setting.gold}G");
                Console.WriteLine();
                Console.WriteLine("[아이템 목록]");
                for (int i = 0; i < setting.gameItems.Length; i++)
                {
                    string item = setting.gameItems[i];
                    string itemInfo = GetItemInfo(item);
                    int itemPrice = GetItemPrice(item);
                    if (setting.myItems.Contains(item))
                    {
                        Console.WriteLine($"{i + 1}. {item} | {itemInfo} | 구매완료");
                    }
                    else
                    {
                        Console.WriteLine($"{i + 1}. {item} | {itemInfo} | {itemPrice} G");
                    }
                    
                }
                Console.WriteLine();
                Console.WriteLine("0. 나가기");
                Console.WriteLine("원하시는 행동을 입력해주세요.");

                string answer = Console.ReadLine();
                int index = int.Parse(answer) - 1;
                if (index >= 0 && index < setting.gameItems.Length)
                {
                    string selectedItem = setting.gameItems[index];
                    int itemPrice = GetItemPrice(selectedItem);

                    if (setting.myItems.Contains(selectedItem))
                    {

                        Console.Clear();
                        Console.WriteLine("이미 아이템을 가지고 있습니다.");
                    }
                    else if (setting.gold >= itemPrice)
                    {
                        setting.myItems.Add(selectedItem);
                        setting.gold -= itemPrice;

                        Console.Clear();
                        Console.WriteLine($"{selectedItem}을(를) 구매했습니다.");
                    }
                    else
                    {

                        Console.Clear();
                        Console.WriteLine("보유한 골드가 부족합니다.");
                    }
                }
                else
                {

                    Console.Clear();
                    Console.WriteLine("잘못된 선택입니다.");
                }

                if (answer == "0")
                {
                    Store();
                }
                else
                {
                    BuyItem();
                }

            }
            static void SellItem()
            {
                Console.WriteLine();
                Console.WriteLine("아이템 판매 중입니다..");
                Console.WriteLine("판매할 아이템 번호를 입력하세요:");
                Console.WriteLine();
                Console.WriteLine("[보유골드]");
                Console.WriteLine($"{setting.gold}G");
                Console.WriteLine();
                Console.WriteLine("[아이템 목록]");
                for (int i = 0; i < setting.myItems.Count; i++)
                {
                    string item = setting.myItems[i];
                    string itemInfo = GetItemInfo(item);
                    float itemSellPrice = GetItemSellPrice(item);
                    Console.WriteLine($"{i + 1}. {item} | {itemInfo} | {itemSellPrice} G");
                }
                Console.WriteLine();
                Console.WriteLine("0. 나가기");
                Console.WriteLine("원하시는 행동을 입력해주세요.");

                string answer = Console.ReadLine();
                if (int.TryParse(answer, out int index))
                {
                    index -= 1;

                    if (index >= 0 && index < setting.myItems.Count)
                    {
                        string selectedItem = setting.myItems[index];
                        float itemSellPrice = GetItemSellPrice(selectedItem);

                        setting.myItems.Remove(selectedItem);
                        setting.gold += itemSellPrice;
                        setting.equippedItems.Remove(selectedItem);



                        Console.Clear();
                        Console.WriteLine($"{selectedItem}을(를) 판매했습니다.");
                    }
                    else if (index == -1) 
                    {
                        Store();
                        return;
                    }
                    else
                    {
                        Console.Clear();
                        Console.WriteLine("잘못된 선택입니다.");
                    }
                }
                else
                {
                    Console.Clear();
                    Console.WriteLine("잘못된 입력입니다.");
                }

                if (answer == "0")
                {
                    Store();
                }
                else
                {
                    SellItem();
                }

            }

            static void Dungeon()
            {
                Console.Clear();
                Console.WriteLine("[던 전]");
                Console.WriteLine("이곳에서 던전으로 들어가기전 활동을 할 수 있습니다.");
                string[] dungeonLevel = { "쉬운", "일반", "어려운" };
                int[] dungeonNeedArmor = { 5, 11, 17 };
                for (int i = 0; i < dungeonLevel.Length; i++)
                {
                    Console.WriteLine($"{i + 1}. {dungeonLevel[i]} 던전 | 방어력 {dungeonNeedArmor[i]} 이상 권장");
                }
                Console.WriteLine("0. 나가기");
                Console.WriteLine("원하시는 행동을 입력해주세요.");

                string answer = Console.ReadLine();
                float totalPower = setting.attack;
                float totalArmor = setting.armor;
                int randomInt = setting.random.Next(0, 5);
                int randomMinusHealth = setting.random.Next(20, 36);
                int randomRewardInt = setting.random.Next(1, 3);
                float[] dungeonGoldReward = { 1000, 1700, 2500 };
                float dungeonReward;

                foreach (string item in setting.equippedItems)
                {
                    totalPower += GetAttackPower(item);
                    totalArmor += GetArmor(item);
                }
                if (setting.health <= 0)
                {
                    Console.Clear();
                    setting.health = 0;
                    Console.WriteLine("체력이없습니다 시작 화면으로 돌아갑니다.");
                    StartScene();
                }

                switch (answer)
                {
                    case "1":
                        HandleDungeonResult(totalArmor, totalPower, dungeonNeedArmor[0], dungeonGoldReward[0], randomInt, randomMinusHealth, randomRewardInt);
                        break;
                    case "2":
                        HandleDungeonResult(totalArmor, totalPower, dungeonNeedArmor[1], dungeonGoldReward[1], randomInt, randomMinusHealth, randomRewardInt);
                        break;
                    case "3":
                        HandleDungeonResult(totalArmor, totalPower, dungeonNeedArmor[2], dungeonGoldReward[2], randomInt, randomMinusHealth, randomRewardInt);
                        break;
                    case "0":
                        StartScene();
                        break;
                    default:
                        Console.WriteLine("다시누르세요");
                        Dungeon();
                        break;
                }
            }

            static void HandleDungeonResult(float totalArmor, float totalPower, int requiredArmor, float rewardGold, int randomInt, int randomMinusHealth, int randomRewardInt)
            {
                if (totalArmor >= requiredArmor)
                {
                    Console.Clear();
                    Console.WriteLine("축하합니다!!");
                    Console.WriteLine("던전을 클리어 하였습니다.");
                    Console.WriteLine("[탐험 결과]");
                    setting.health -= randomMinusHealth + (requiredArmor - totalArmor);
                    float dungeonReward = (rewardGold + (totalPower * rewardGold * 0.01f * randomRewardInt));
                    setting.gold += dungeonReward;
                    levelUP();
                    Console.WriteLine($"{dungeonReward}G");
                    Console.WriteLine("0. 나가기");
                    string answer = Console.ReadLine();
                    if (answer == "0")
                    {
                        StartScene();
                    }
                }
                else
                {
                    if (randomInt < 2) // 40% 확률로 성공, 60% 확률로 사망
                    {
                        Console.Clear();
                        Console.WriteLine("축하합니다!!");
                        Console.WriteLine("던전을 클리어 하였습니다.");
                        Console.WriteLine("[탐험 결과]");
                        setting.health -= randomMinusHealth + (requiredArmor - totalArmor);
                        float dungeonReward = (rewardGold + (totalPower * rewardGold * 0.1f * randomRewardInt));
                        setting.gold += dungeonReward;
                        levelUP();
                        Console.WriteLine($"{dungeonReward}G");
                        Console.WriteLine("0. 나가기");
                        Console.WriteLine("원하시는 행동을 입력해주세요.");
                        string answer = Console.ReadLine();
                        if (answer == "0")
                        {
                            StartScene();
                        }
                    }
                    else
                    {
                        HandleDeath();
                    }
                }
            }

            static void HandleDeath()
            {
                Console.Clear();
                Console.WriteLine("죽었습니다.");
                setting.health -= setting.health / 2;
                Console.WriteLine("아무 키나 누르면 시작화면으로 돌아갑니다.");
                string answer = Console.ReadLine();
                StartScene();
            }

            static void TakeRest()
            {

                Console.Clear();
                Console.WriteLine("[휴식하기]");
                Console.WriteLine($"500 G 를 내면 체력을 회복할 수 있습니다. (보유 골드 : {setting.gold}G)");
                Console.WriteLine("1. 휴식하기");
                Console.WriteLine("0. 나가기");
                Console.WriteLine("원하시는 행동을 입력해주세요.");
                string answer = Console.ReadLine();
                if (answer == "1")
                {
                    if (setting.gold >= 500)
                    {
                        Console.Clear();
                        setting.health = 100;
                        setting.gold -= 500;
                        Console.WriteLine("휴식을 완료했습니다");
                        Console.WriteLine("0. 나가기");
                        Console.WriteLine("원하시는 행동을 입력해주세요.");
                        answer = Console.ReadLine();
                        if (answer == "0") StartScene();
                    }
                    else
                    {
                        Console.Clear();
                        Console.WriteLine("Gold 가 부족합니다.");
                        Console.WriteLine("쫒겨났습니다.");
                        StartScene();
                    }
                }
                else if (answer == "0")
                {
                    StartScene();
                }
            }

            static void levelUP()
            {
                setting.dungeonclearCnt++;
                Console.WriteLine($"레벨업하셨습니다!! Lv. {setting.level} -> Lv. {setting.level + 1}");
                setting.armor += 0.5f;
                setting.attack +=  1f; 
                setting.level++;
            }

            static string GetItemInfo(string itemName)
            {
                switch (itemName)
                {
                    case "수련자 갑옷":
                        return "방어력 +5 | 수련에 도움을 주는 갑옷입니다.";
                    case "무쇠갑옷":
                        return "방어력 +9 | 무쇠로 만들어져 튼튼한 갑옷입니다.";
                    case "스파르타의 갑옷":
                        return "방어력 +15 | 스파르타의 전사들이 사용했다는 전설의 갑옷입니다.";
                    case "낡은 검":
                        return "공격력 +2 | 쉽게 볼 수 있는 낡은 검 입니다.";
                    case "청동 도끼":
                        return "공격력 +5 | 어디선가 사용됐던거 같은 도끼입니다.";
                    case "스파르타의 창":
                        return "공격력 +7 | 스파르타의 전사들이 사용했다는 전설의 창입니다.";
                    default:
                        return "정보 없음";
                }
            }

            static int GetItemPrice(string itemName)
            {
                switch (itemName)
                {
                    case "수련자 갑옷":
                        return 1000;
                    case "무쇠갑옷":
                        return 1500;
                    case "스파르타의 갑옷":
                        return 3500;
                    case "낡은 검":
                        return 600;
                    case "청동 도끼":
                        return 1500;
                    case "스파르타의 창":
                        return 2500;
                    default:
                        return 0;
                }
            }
            static float GetItemSellPrice(string itemName)
            {
                switch (itemName)
                {
                    case "수련자 갑옷":
                        return 1000 / 2.5f;
                    case "무쇠갑옷":
                        return 1500 / 2.5f;
                    case "스파르타의 갑옷":
                        return 3500 / 2.5f;
                    case "낡은 검":
                        return 600 / 2.5f;
                    case "청동 도끼":
                        return 1500 / 2.5f;
                    case "스파르타의 창":
                        return 2500 / 2.5f;
                    default:
                        return 0;
                }
            }

            static void ToggleEquipStatus(string itemName) //아이템 토글
            {
                if (setting.equippedItems.Contains(itemName))
                {
                    setting.equippedItems.Remove(itemName);
                }
                else
                {
                    string itemType = CheckType(itemName);
                    if (!HasEquippedItemType(itemType))
                    {
                        setting.equippedItems.Add(itemName);
                    }
                    else
                    {
                        Console.WriteLine("이미 같은 종류의 아이템을 장착하고 있습니다.");
                    }
                }
            }

            static bool HasEquippedItemType(string itemType)
            {
                foreach (string equippedItem in setting.equippedItems)
                {
                    string type = CheckType(equippedItem);
                    if (type == itemType)
                    {
                        return true;
                    }
                }
                return false;
            }
            static bool IsItemEquipped(string itemName) // 장착 여부 확인
            {
                return setting.equippedItems.Contains(itemName);
            }
            static string CheckType(string itemName)
            {
                switch (itemName)
                {
                    case "수련자 갑옷":
                        return "방어구류";
                    case "무쇠갑옷":
                        return "방어구류";
                    case "스파르타의 갑옷":
                        return "방어구류";
                    case "낡은 검":
                        return "무기류";
                    case "청동 도끼":
                        return "무기류";
                    case "스파르타의 창":
                        return "무기류";
                    default:
                        return "";
                }
            }

            static int GetArmor(string itemName)
            {
                switch (itemName)
                {
                    case "수련자 갑옷":
                        return 5;
                    case "무쇠갑옷":
                        return 9;
                    case "스파르타의 갑옷":
                        return 15;
                    default:
                        return 0;
                }
            }
            static int GetAttackPower(string itemName)
            {
                switch (itemName)
                {
                    case "낡은 검":
                        return 2;
                    case "청동 도끼":
                        return 5;
                    case "스파르타의 창":
                        return 7;
                    default:
                        return 0;
                }
            }
        }
    }
}