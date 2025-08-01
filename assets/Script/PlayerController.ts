import { _decorator, Component, Node, Vec3 } from 'cc';
const { ccclass, property } = _decorator;

@ccclass('PlayerController')
export class PlayerController extends Component {
    @property
    moveSpeed: number = 5;

    @property(Node)
    cameraNode: Node = null;

    private _inputDir: Vec3 = new Vec3();

    public setMoveDirection(dir: Vec3) {
        this._inputDir.set(dir);
    }

    update(dt: number) {
        if (!this.cameraNode) return;

        const camMat = this.cameraNode.worldMatrix;

        // Forward của camera (đảo lại vì model quay về -Z)
        const camForward = new Vec3(camMat.m08, 0, camMat.m10);
        camForward.normalize();

        const camRight = new Vec3(camMat.m00, 0, camMat.m02);
        camRight.normalize();

        // Di chuyển nếu có input joystick
        if (this._inputDir.lengthSqr() > 0.001) {
            const moveDir = new Vec3();
            Vec3.scaleAndAdd(moveDir, moveDir, camForward, this._inputDir.z);
            Vec3.scaleAndAdd(moveDir, moveDir, camRight, this._inputDir.x);

            moveDir.normalize();
            moveDir.multiplyScalar(this.moveSpeed * dt);

            const currentPos = this.node.worldPosition;
            Vec3.add(currentPos, currentPos, moveDir);
            this.node.setWorldPosition(currentPos);
        }

        // Luôn xoay player theo hướng camera (dù có di chuyển hay không)
        if (camForward.lengthSqr() > 0.001) {
            const angle = Math.atan2(-camForward.x, -camForward.z) * 180 / Math.PI;
            this.node.eulerAngles = new Vec3(0, angle, 0);
        }
    }

}
