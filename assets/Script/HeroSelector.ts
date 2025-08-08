import { _decorator, Component, Node, Prefab, instantiate, Vec3, Label } from 'cc';
const { ccclass, property } = _decorator;

enum TileType {
    None = 0,
    Grass = 1,
    Stone = 2,
    Water = 3,
    Path = 4,
}

@ccclass('HeroSelector')
export class HeroSelector extends Component {
    @property([Prefab])
    heroPrefabs: Prefab[] = [];

    @property(Node)
    playerNode: Node = null; // Node Player trên map 3D

    @property(Node)
    heroSpawnParent: Node = null; // Node cha chứa các hero trên map 3D

    @property(Label)
    countLabel: Label = null;

    @property
    maxHeroes: number = 5; // Giới hạn số hero tối đa

    private _remainingHero: number = 0;
    private _heroCount: number = this.maxHeroes;

    start() {
        // Lấy node content
        const content = this.node
            .getChildByName("view")
            ?.getChildByName("content");

        if (!content) {
            console.warn("❌ Không tìm thấy content trong ScrollView");
            return;
        }

        // Gán sự kiện cho từng icon hero
        content.children.forEach((item, index) => {
            item.on(Node.EventType.TOUCH_END, () => {
                this.spawnHero(index);
            });
        });

        this.updateCountLabel();
    }

    spawnHero(index: number) {
        if (this._heroCount == 0) {
            console.warn(`⚠️ Đã đạt giới hạn ${this._heroCount} hero`);
            return;
        }

        if (!this.isPlayerOnAllowedTile()) {
            console.warn("⚠️ Chỉ được đặt hero trên Grass hoặc Water");
            return;
        }

        if (!this.playerNode) {
            console.warn("❌ playerNode chưa được gán");
            return;
        }

        const prefab = this.heroPrefabs[index];
        if (!prefab) {
            console.warn(`❌ Không tìm thấy prefab hero ở index ${index}`);
            return;
        }

        // Tạo hero từ prefab
        const hero = instantiate(prefab);
        hero.setParent(this.heroSpawnParent || this.playerNode.parent);

        // Lấy tọa độ world của player
        const spawnPos = this.playerNode.worldPosition.clone();
        spawnPos.y = this.playerNode.position.y; // đảm bảo hero sát mặt đất
        hero.setWorldPosition(spawnPos);

        this._remainingHero++;
        this.updateCountLabel();


    }

    isPlayerOnAllowedTile(): boolean {
        // 🔹 Giả sử bạn đã có hàm getTileTypeAtWorldPos(worldPos) trả về TileType
        const playerPos = this.playerNode.worldPosition.clone();
        const tileType = this.getTileTypeAtWorldPos(playerPos);

        return tileType === TileType.Grass || tileType === TileType.Water;
    }

    getTileTypeAtWorldPos(worldPos: Vec3): TileType {
        // 📌 Ở đây mình sẽ để giả lập
        // Bạn cần thay bằng code lấy tile thực tế từ hệ thống map của bạn
        // Ví dụ: return this.mapManager.getTileType(worldPos);

        // Tạm thời test: cứ cho là Grass
        return TileType.Grass;
    }

    updateCountLabel() {
        if (this.countLabel) {
            this._heroCount = this.maxHeroes - this._remainingHero;
            this.countLabel.string = this._heroCount.toString();
        }
    }
}
