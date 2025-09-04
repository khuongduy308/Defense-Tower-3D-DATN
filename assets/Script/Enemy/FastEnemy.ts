import { _decorator, Component, Node } from 'cc';
import { BaseEnemy } from './BaseEnemy';
const { ccclass, property } = _decorator;

@ccclass('FastEnemy')
export class FastEnemy extends BaseEnemy {
    @property moveSpeed: number = 2;
    @property maxHp: number = 100;
}

