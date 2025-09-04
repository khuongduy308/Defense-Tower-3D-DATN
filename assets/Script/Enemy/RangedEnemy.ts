import { _decorator, Component, Node, Prefab, instantiate, Vec3, tween, v3, Layers, find } from 'cc';
import { BaseEnemy } from './BaseEnemy';
import { BaseHero } from '../Hero/BaseHero';
const { ccclass, property } = _decorator;

@ccclass('RangedEnemy')
export class RangedEnemy extends BaseEnemy {

    @property(Prefab)
    arrowPrefab: Prefab = null; // Prefab mũi tên

    @property
    attackRange: number = 5; // Phạm vi bắn

    @property
    attackDamage: number = 10; //Sat thuong moi phat ban

    @property
    attackInterval: number = 2; // Thời gian giữa các lần bắn (giây)

    private _attackTimer: number = 0;
    private _targetHero: Node = null;


    update(deltaTime: number) {
        super.update?.(deltaTime); // Nếu BaseEnemy có update

        // Giảm timer
        this._attackTimer -= deltaTime;

        this._targetHero = this.findNearestHero();
        // console.log('RangedEnemy tìm thấy hero:', this._targetHero ? this._targetHero.name : 'Không có hero gần');

        // Nếu có hero và trong phạm vi thì bắn
        if (this._targetHero && this.isHeroInRange()) {
            if (this._attackTimer <= 0) {
                this.shootArrow();
                this._attackTimer = this.attackInterval;
            }
        }
    }

    findNearestHero(): Node | null {
        const HERO_LAYER = Layers.nameToLayer('HeroLayer');
        let nearestHero: Node | null = null;
        let minDist = this.attackRange;
        const myPos = this.node.worldPosition;

        const heroRoot = find('Main/Hero');

        if (!heroRoot) return null;

        for (const hero of heroRoot.children) {
            // console.log(hero.layer);
            if (!hero.isValid) {
                console.log(`RangedEnemy phát hiện hero không hợp lệ: ${hero.name}`);
                continue;
            }

            // if (hero.layer !== HERO_LAYER) {
            //     console.log('hero ko co layer')
            //     continue;
            // }

            const heroComp = hero.getComponent(BaseHero);
            if (!heroComp || heroComp.getHp() <= 0) {
                console.log('hero ko co BaseHero hoac ko du HP');
                continue;
            }

            console.log(`RangedEnemy tìm thấy hero: ${hero.name} với HP: ${heroComp.getHp()}`);

            const dist = Vec3.distance(myPos, hero.worldPosition);
            if (dist <= this.attackRange && dist < minDist) {
                minDist = dist;
                nearestHero = hero;
            }
        }

        return nearestHero;
    }

    isHeroInRange(): boolean {
        if (!this._targetHero) return false;
        const dist = Vec3.distance(this.node.worldPosition, this._targetHero.worldPosition);
        return dist <= this.attackRange;
    }

    shootArrow() {
        if (!this.arrowPrefab || !this._targetHero || !this._targetHero.isValid) return;

        const arrow = instantiate(this.arrowPrefab);
        arrow.setWorldPosition(this.node.worldPosition);
        this.node.parent.addChild(arrow);

        const targetPos = this._targetHero.worldPosition.clone();

        const moveDuration = 0.3; // Thời gian bay
        tween(arrow)
            .to(moveDuration, { position: targetPos })
            .delay(0.5)
            .call(() => {
                if (this._targetHero && this._targetHero.isValid) {
                    const heroComp = this._targetHero.getComponent(BaseHero);
                    if (heroComp) {
                        heroComp.takeDamage(this.attackDamage);
                        // console.log(`🏹 ${this.node.name} bắn trúng ${this._targetHero.name}, hero còn ${heroComp.getHp()} HP`);
                    }
                }
                if (arrow.isValid) arrow.destroy();
            })
            .start();
    }
}
