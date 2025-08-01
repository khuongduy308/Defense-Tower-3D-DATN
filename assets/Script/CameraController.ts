import { _decorator, Component, Node, Vec3, Input, input, EventTouch } from 'cc';
const { ccclass, property } = _decorator;

@ccclass('CameraController')
export class CameraController extends Component {
    @property(Node)
    target: Node = null;

    @property
    distance: number = 6;

    @property
    height: number = 3;

    @property
    rotateSpeed: number = 0.3; // tốc độ xoay camera theo touch

    private _yaw: number = 0; // góc xoay ngang quanh Y
    private _camPos: Vec3 = new Vec3();
    private _tempVec3: Vec3 = new Vec3();

    onLoad() {
        input.on(Input.EventType.TOUCH_MOVE, this.onTouchMove, this);

        // Thiết lập camera ngay vị trí ban đầu khi load
        this.setupInitialCameraPosition();
    }

    onDestroy() {
        input.off(Input.EventType.TOUCH_MOVE, this.onTouchMove, this);
    }

    private onTouchMove(event: EventTouch) {
        const delta = event.getDelta();
        this._yaw -= delta.x * this.rotateSpeed; // chỉ xoay theo trục ngang
    }

    private setupInitialCameraPosition() {
        if (!this.target) return;

        // Cố định camera ở phía sau player theo yaw ban đầu (0 độ)
        const rad = this._yaw * Math.PI / 180;
        const offsetX = Math.sin(rad) * this.distance;
        const offsetZ = Math.cos(rad) * this.distance;

        const targetPos = this.target.worldPosition;
        this._camPos.set(targetPos.x + offsetX, targetPos.y + this.height, targetPos.z + offsetZ);
        this.node.setWorldPosition(this._camPos);
        this.node.lookAt(targetPos);
    }

    lateUpdate() {
        if (!this.target) return;

        const targetPos = this.target.worldPosition;

        const rad = this._yaw * Math.PI / 180;
        const offsetX = Math.sin(rad) * this.distance;
        const offsetZ = Math.cos(rad) * this.distance;

        // Vị trí camera cách player 1 khoảng cố định, xoay quanh theo _yaw
        this._camPos.set(
            targetPos.x + offsetX,
            targetPos.y + this.height,
            targetPos.z + offsetZ
        );

        this.node.setWorldPosition(this._camPos);
        this.node.lookAt(targetPos);
    }
}
