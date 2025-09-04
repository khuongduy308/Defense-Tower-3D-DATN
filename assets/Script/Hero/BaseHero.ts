import { _decorator, Component, Node, Vec3, director } from 'cc';
const { ccclass, property } = _decorator;
import { BaseEnemy } from '../Enemy/BaseEnemy';

@ccclass('BaseHero')
export class BaseHero extends Component {
    @property maxHp: number = 100;
    @property maxEnergy: number = 100;
    @property attackRange: number = 5;
    @property attackDamage: number = 10;
    @property attackCooldown: number = 1;

    private currentHp: number = 0;
    private currentEnergy: number = 0;
    private cooldownTimer: number = 0;

    start() {
        this.currentHp = this.maxHp;
        this.currentEnergy = this.maxEnergy;
    }

    update(dt: number) {
        this.cooldownTimer -= dt;
        if (this.cooldownTimer <= 0) {
            const target = this.findClosestEnemy();
            if (target) {
                this.attack(target);
                this.cooldownTimer = this.attackCooldown;
            }
        }
    }

    findClosestEnemy(): Node | null {
        const enemies = this.getAllEnemiesInScene();
        let minDist = Infinity;
        let closest: Node | null = null;
        const myPos = this.node.worldPosition;

        for (const enemy of enemies) {
            const dist = Vec3.distance(myPos, enemy.worldPosition);
            if (dist <= this.attackRange && dist < minDist) {
                minDist = dist;
                closest = enemy;
            }
        }
        return closest;
    }

    getAllEnemiesInScene(): Node[] {
        const scene = director.getScene();
        const enemiesNode = scene.getChildByName("Main")?.getChildByName("Enemies");
        if (!enemiesNode) return [];
        // console.log(`Found ${enemiesNode.children.length} enemies in scene`);
        return enemiesNode.children;
    }

    attack(enemy: Node): void {
        const enemyHealth = enemy.getComponent(BaseEnemy); // 👈 dùng class
        // console.log(`Hero attacking ${enemy.name} at position ${enemy.worldPosition}`);
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

    // // Nếu muốn dùng năng lượng để kích hoạt skill:
    // useEnergy(amount: number): boolean {
    //     if (this.currentEnergy >= amount) {
    //         this.currentEnergy -= amount;
    //         return true;
    //     }
    //     return false;
    // }

    // Getter để UI hiển thị
    getHp(): number {
        return this.currentHp;
    }

    getEnergy(): number {
        return this.currentEnergy;
    }
}
