import { _decorator, Component, Node, Prefab, instantiate, Vec3, EventTouch } from 'cc';
const { ccclass, property } = _decorator;

@ccclass('HeroSelector')
export class HeroSelector extends Component {
    @property([Prefab]) heroPrefabs: Prefab[] = [];
    @property(Node) contentNode: Node = null;
    @property(Node) playerNode: Node = null;
    @property(Node) spawnParent: Node = null;

    start() {
        if (!this.contentNode) {
            console.error("🚫 Thiếu contentNode trong ScrollView!");
            return;
        }

        this.contentNode.children.forEach((child, index) => {
            child.on(Node.EventType.TOUCH_END, (event: EventTouch) => {
                console.log(`🧙 Clicked hero item index ${index}`);
                this.spawnHero(index);
            }, this);
        });
    }

    spawnHero(index: number) {
        const prefab = this.heroPrefabs[index];
        if (!prefab) {
            console.warn(`❌ Prefab không tồn tại tại index ${index}`);
            return;
        }

        const hero = instantiate(prefab);
        hero.setParent(this.spawnParent || this.playerNode.parent);

        const spawnPos = this.playerNode.worldPosition.clone();
        spawnPos.y = 0.1;
        hero.setWorldPosition(spawnPos);

        console.log(`✅ Spawned hero ${hero.name} tại ${spawnPos}`);
    }
}
