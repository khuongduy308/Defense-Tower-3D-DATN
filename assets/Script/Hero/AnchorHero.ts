import { _decorator, Node, Prefab, Component, instantiate, Vec3, tween } from 'cc';
import { BaseHero } from './BaseHero';
import { BaseEnemy } from '../Enemy/BaseEnemy';
import { director } from 'cc';

const { ccclass, property } = _decorator;

@ccclass('AnchorHero')
export class AnchorHero extends BaseHero {
    @property({ type: Prefab }) arrowPrefab: Prefab = null;

    @property
    arrowCooldown: number = 1; // thời gian hồi 1 giây

    private arrowTimer: number = 0;

    update(deltaTime: number) {

        this.arrowTimer -= deltaTime;

        const enemies = this.findEnemiesInRange();
        if (enemies.length > 0 && this.arrowTimer <= 0) {
            this.arrowTimer = this.arrowCooldown;
            const target = enemies[0];
            if (target && target.isValid) {
                this.attack(target);
            }
        }
    }

    attack(target: Node) {
        console.log(this.node.name +  ' bắn tên vào ' + target.name);
        this.shootArrow(target);
    }

    shootArrow(target: Node) {
        if (!this.arrowPrefab) return;

        const arrow = instantiate(this.arrowPrefab);
        arrow.setParent(this.node.parent); // cùng tầng với hero

        const startPos = this.node.worldPosition;
        const endPos = target.worldPosition.clone();
        endPos.y = startPos.y; // 👈 Giữ y để mũi tên bay ngang
        arrow.setWorldPosition(startPos);

        // Có thể xoay mũi tên theo hướng bay
        const dir = new Vec3();
        Vec3.subtract(dir, endPos, startPos);
        arrow.lookAt(endPos); // xoay đầu mũi tên theo hướng bay

        const distance = Vec3.distance(startPos, endPos);
        const flightTime = distance / 10; // tốc độ bay (tùy chỉnh)

        tween(arrow)
            .to(flightTime, { position: endPos })
            .call(() => {
                const enemy = target.getComponent(BaseEnemy);
                
                // ✅ Đảm bảo enemy còn tồn tại và chưa bị destroy
                if (target && target.isValid && enemy) {
                    enemy.takeDamage(this.attackDamage);
                }

                arrow.destroy();
            })
            .start();
    }

    isInRange(a: Vec3, b: Vec3, range: number): boolean {
        const dx = a.x - b.x;
        const dz = a.z - b.z;
        const distXZ = Math.sqrt(dx * dx + dz * dz);
        return distXZ <= range;
    }

    findEnemiesInRange(): Node[] {
        const enemiesInScene = this.getAllEnemiesInScene();
        const myPos = this.node.worldPosition;

        return enemiesInScene.filter(enemy => {
            const enemyPos = enemy.worldPosition;
            const dx = myPos.x - enemyPos.x;
            const dz = myPos.z - enemyPos.z;
            const distanceXZ = Math.sqrt(dx * dx + dz * dz);
            return distanceXZ <= this.attackRange;
        });
    }

    getAllEnemiesInScene(): Node[] {
        const scene = director.getScene();
        if (!scene) {
            console.warn("❌ Không lấy được scene từ director");
            return [];
        }

        const enemiesNode = scene.getChildByName("Main")?.getChildByName("Enemies");

        if (!enemiesNode) {
            console.warn("❌ Không tìm thấy node Enemies trong scene");
            return [];
        }

        console.log(`✅ Found ${enemiesNode.children.length} enemies in scene`);
        return enemiesNode.children;
    }
}
