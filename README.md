# starve-io
---
### 目前資料夾架構
```
├─Assets
│  ├─Materials
│  ├─Prefabs
│  ├─Scenes
│  ├─Scripts
│  │  ├─Core
│  │  ├─Data
│  │  ├─Entities
│  │  │  ├─Mobs
│  │  │  └─Player
│  │  ├─Input
│  │  ├─System
│  │  │  ├─Building
│  │  │  ├─Crafting
│  │  │  ├─Farming
│  │  │  └─Inventory
│  │  ├─UI
│  │  └─World
│  ├─Settings
│  │  └─Scenes
│  ├─Sprites
│  │  ├─characters
│  │  ├─objects
│  │  ├─palettes
│  │  ├─particles
│  │  └─tilesets
│  │      ├─floors
│  │      ├─Map _ Starve.io Wiki _ Fandom
│  │      └─walls
│  ├─Tiles
│  └─UI Toolkit
│      └─UnityThemes
├─Library
```
---
### TODO
* #### Prefabs
    找更多素材(?)
    目前來源:https://starveiopro.fandom.com/wiki/Starveio_Wiki:Home
*  #### Scripts
    ##### 1. Core (核心)
    - [ ] **TimeManager.cs**: 日夜循環控制 (已完成搬遷)
    - [ ] **GameManager.cs**: 遊戲流程控制 (Start, Pause, GameOver)
    - [ ] **ObjectPooler.cs**: 物件池 (優化掉落物與怪物生成)

    ##### 2. Inputs (輸入)
    - [ ] **InputReader.cs**: 封裝 Input System 事件，需修改`PlayerController.cs`

    ##### 3. Entities (實體)
    - [ ] **PlayerStats.cs**: 玩家數值 (血量/飢餓/體溫/口渴)
    - [x] **PlayerController.cs**: **已完成移動、旋轉**，剩下互動邏輯
    - [ ] **MobBase.cs**: 生物共用基類 (血量/受傷)
    - [ ] **MobAI.cs**: 基礎行為狀態機

    ##### 4. Systems (系統)
    - [ ] **Inventory System**:
        - [ ] **ItemData.cs** (ScriptableObject)
        - [ ] **InventorySystem.cs** (核心邏輯)
    - [ ] **Crafting System**:
        - [ ] **RecipeData.cs** (ScriptableObject)
        - [ ] **CraftingManager.cs** (合成邏輯)
    - [ ] **Building System**:
        - [ ] **BuildingPlacement.cs** (放置檢測)
    - [ ] **Farming System**:
        - [ ] **CropData.cs** (作物資料)

    ##### 5. UI (介面)
    - [ ] **HUDManager.cs**: 連結 PlayerStats 更新 UI Toolkit
    - [ ] **InventoryUI.cs**: 監聽背包數據並刷新
    - [ ] **SlotUI.cs**: 格子互動邏輯

    ##### 6. World (世界)
    - [x] **MapGenerator.cs**: 地圖生成，目前固定上雪下草，未來可考慮加入更多生態域
    - [ ] **ResourceNode.cs**: 資源點 (樹/石頭) 掉落邏輯
* ### UI
    - [x] 物品欄
    - [x] 狀態欄
    - [ ] 左上角合成欄
    - [ ] 右邊合成表按鈕及合成表
    - [ ] 右邊日夜變化表
* ### Other
    1. **優化FPS**
    2. **優化生成連續資源**: 石頭和樹貼在一起之類的
    3. **存檔功能**: 類似minecraft那樣
...
