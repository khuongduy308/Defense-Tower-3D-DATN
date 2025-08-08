import { _decorator, Component, Node, Vec3 } from 'cc';
const { ccclass, property } = _decorator;
import { BaseEnemy } from '../Enemy/BaseEnemy';

@ccclass('BaseHero')
export class BaseHero extends Component {
    @property
    maxHp: number = 100;

    @property
    maxEnergy: number = 100;

    @property
    attackRange: number = 5;

    @property
    attackDamage: number = 10;

    private currentHp: number = 0;
    private currentEnergy: number = 0;

    start() {
        this.currentHp = this.maxHp;
        this.currentEnergy = this.maxEnergy;
    }

    update(deltaTime: number) {
        const enemies = this.findEnemiesInRange();
        if (enemies.length > 0) {
            this.attack(enemies[0]);
        }
    }

    findEnemiesInRange(): Node[] {
        const enemiesInScene = this.getAllEnemiesInScene();
        const myPos = this.node.worldPosition;

        return enemiesInScene.filter(enemy => {
            return Vec3.distance(myPos, enemy.worldPosition) <= this.attackRange;
        });
    }

    getAllEnemiesInScene(): Node[] {

        const enemiesNode = this.node.scene.getChildByName("Enemies");
        if (!enemiesNode) return [];
        console.log(`Found ${enemiesNode.children.length} enemies in scene`);
        return enemiesNode.children;
    }

    attack(enemy: Node): void {
    const enemyHealth = enemy.getComponent(BaseEnemy); // 👈 dùng class
    console.log(`Hero attacking ${enemy.name} at position ${enemy.worldPosition}`);
    if (enemyHealth) {
        enemyHealth.takeDamage(this.attackDamage);
    }
}

    takeDamage(damage: number): void {
        this.currentHp -= damage;
        if (this.currentHp <= 0) {
            this.currentHp = 0;
            this.onDeath();
        }
    }

    gainHp(amount: number): void {
        this.currentHp = Math.min(this.currentHp + amount, this.maxHp);
    }

    gainEnergy(amount: number): void {
        this.currentEnergy = Math.min(this.currentEnergy + amount, this.maxEnergy);
    }

    onDeath(): void {
        this.node.destroy();
    }

    // Nếu muốn dùng năng lượng để kích hoạt skill:
    useEnergy(amount: number): boolean {
        if (this.currentEnergy >= amount) {
            this.currentEnergy -= amount;
            return true;
        }
        return false;
    }

    // Getter để UI hiển thị
    getHp(): number {
        return this.currentHp;
    }

    getEnergy(): number {
        return this.currentEnergy;
    }
}
