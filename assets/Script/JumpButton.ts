import { _decorator, Component, Node, Button } from 'cc';
import { PlayerController } from './PlayerController';
const { ccclass, property } = _decorator;

@ccclass('JumpButton')
export class JumpButton extends Component {
    @property(Node)
    playerNode: Node = null; // Kéo Player node vào đây trong Editor

    private _playerController: PlayerController = null;

    onLoad() {
        if (!this.playerNode) {
            console.warn("❗️Chưa gán Player Node trong JumpButton!");
            return;
        }

        this._playerController = this.playerNode.getComponent(PlayerController);

        if (!this._playerController) {
            console.warn("❗️Player node không có PlayerController!");
        }
    }

    // Hàm này sẽ được gọi khi nhấn nút (gán sự kiện OnClick trong Editor)
    onJumpPressed() {
        if (this._playerController) {
            this._playerController.jump();
        }
    }
}
