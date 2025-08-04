import { _decorator, Component, Node, EventTouch, Vec2, UITransform, Vec3, input, Input } from 'cc';
import { PlayerController } from './PlayerController';

const { ccclass, property } = _decorator;

@ccclass('Joystick')
export class Joystick extends Component {
    @property(Node)
    joystickHandle: Node = null;

    @property
    maxDistance: number = 10;

    @property
    moveSpeed: number = 1;

    @property(PlayerController)
    playerController: PlayerController = null; // Gắn vào từ Inspector

    private _joystickCenter: Vec3 = new Vec3();
    private _moveDirection: Vec3 = new Vec3();
    private _isPressed: boolean = false;

    onLoad() {
        this._joystickCenter = this.node.position.clone();

        this.node.on(Input.EventType.TOUCH_START, this.onTouchStart, this);
        this.node.on(Input.EventType.TOUCH_MOVE, this.onTouchMove, this);
        this.node.on(Input.EventType.TOUCH_END, this.onTouchEnd, this);
        this.node.on(Input.EventType.TOUCH_CANCEL, this.onTouchEnd, this);
    }

    onTouchStart(event: EventTouch) {
        this._isPressed = true;
        // this.updateJoystick(event);
    }

    onTouchMove(event: EventTouch) {
        if (this._isPressed) {
            this.updateJoystick(event);
        }
    }

    onTouchEnd(event: EventTouch) {
        this._isPressed = false;
        this.joystickHandle.position = Vec3.ZERO;
        this._moveDirection.set(0, 0, 0);

        // Gửi về player dừng lại
        this.playerController?.setMoveDirection(new Vec3());
    }

    updateJoystick(event: EventTouch) {
        const uiTransform = this.node.getComponent(UITransform)!;
        const touchPos = new Vec3();
        uiTransform.convertToNodeSpaceAR(new Vec3(event.getLocation().x, event.getLocation().y, 0), touchPos);

        let distance = touchPos.length();
        if (distance > this.maxDistance) {
            touchPos.normalize().multiplyScalar(this.maxDistance);
            distance = this.maxDistance;
        }

        this.joystickHandle.position = new Vec3(touchPos.x, touchPos.y, 0);

        this._moveDirection.set(
            touchPos.x / this.maxDistance,
            0,
            -touchPos.y / this.maxDistance
        );

        // Gửi hướng và tốc độ về Player
        this.playerController.setMoveDirection(this._moveDirection);
    }

    onDestroy() {
        this.node.off(Input.EventType.TOUCH_START, this.onTouchStart, this);
        this.node.off(Input.EventType.TOUCH_MOVE, this.onTouchMove, this);
        this.node.off(Input.EventType.TOUCH_END, this.onTouchEnd, this);
        this.node.off(Input.EventType.TOUCH_CANCEL, this.onTouchEnd, this);
    }
}
