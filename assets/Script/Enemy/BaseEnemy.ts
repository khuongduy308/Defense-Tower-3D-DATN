import { _decorator, Component, Vec3, Node, tween, Animation } from 'cc';
const { ccclass, property } = _decorator;

@ccclass('BaseEnemy')
export class BaseEnemy extends Component {
    @property moveSpeed: number = 2;
    @property maxHp: number = 100;

    private path: Vec3[] = [];
    private currentIndex: number = 0;
    private currentHp: number = 0;

    onLoad() {
        this.currentHp = this.maxHp;
    }

    init(path: Vec3[]) {
        this.path = path;
        this.currentIndex = 0;
        this.moveToNextPoint();
    }

    moveToNextPoint() {
        if (this.currentIndex >= this.path.length) {
            this.onReachEnd();
            return;
        }

        this.node.getComponent(Animation)?.play('walk');

        const targetPos = this.path[this.currentIndex];
        targetPos.y = 0.1; // 👈 Đảm bảo đi trên mặt đường

        // 👉 Tính hướng di chuyển
        const currentPos = this.node.worldPosition.clone();
        const dir = new Vec3();
        Vec3.subtract(dir, targetPos, currentPos);
        dir.normalize();

        // 👉 Tính góc quay quanh trục Y (theo hướng di chuyển)
        const angleY = Math.atan2(dir.x, dir.z); // (x,z) vì trục Y là hướng đứng
        this.node.eulerAngles = new Vec3(0, angleY * 180 / Math.PI, 0);

        tween(this.node)
            .to(Vec3.distance(this.node.worldPosition, targetPos) / this.moveSpeed, {
                position: targetPos
            })
            .call(() => {
                this.currentIndex++;
                this.moveToNextPoint();
            })
            .start();
    }

    
    takeDamage(damage: number) {
        this.currentHp -= damage;
        // console.log(`💥 ${this.node.name} bị tấn công, còn ${this.currentHp} HP`);

        if (this.currentHp <= 0) {
            this.onDeath();
        }
    }

    onDeath() {
        // console.log(`☠️ ${this.node.name} đã chết`);
        this.node.destroy();
    }
    
    onReachEnd() {
        console.log("❗ Enemy reached goal!");
        this.node.destroy();
    }
}

