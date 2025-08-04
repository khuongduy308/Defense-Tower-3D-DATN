import { _decorator, Component, Node, Vec3, Animation, RigidBody, SkeletalAnimation } from 'cc';
const { ccclass, property } = _decorator;

@ccclass('PlayerController')
export class PlayerController extends Component {
    @property
    moveSpeed: number = 1;

    @property
    jumpForce: number = 10;

    @property(Node)
    cameraNode: Node = null;


    private _inputDir: Vec3 = new Vec3();
    private _animation: Animation = null;
    private _rigidBody: RigidBody = null;
    private _canJump: boolean = true;
    private _currentAnim: string = '';

    onLoad() {
        this._animation = this.getComponent(SkeletalAnimation);
        this._rigidBody = this.getComponent(RigidBody);
    }

    public setMoveDirection(dir: Vec3) {
        this._inputDir.set(dir);
    }

    update(dt: number) {
        if (!this.cameraNode || !this._rigidBody) return;

        const camMat = this.cameraNode.worldMatrix;
        const camForward = new Vec3(camMat.m08, 0, camMat.m10);
        const camRight = new Vec3(camMat.m00, 0, camMat.m02);
        camForward.normalize();
        camRight.normalize();

        const moveDir = new Vec3();
        if (this._inputDir.lengthSqr() > 0.001) {
            Vec3.scaleAndAdd(moveDir, moveDir, camForward, this._inputDir.z);
            Vec3.scaleAndAdd(moveDir, moveDir, camRight, this._inputDir.x);
            moveDir.normalize().multiplyScalar(this.moveSpeed);

            const velocity = new Vec3();
            this._rigidBody.getLinearVelocity(velocity);
            this._rigidBody.setLinearVelocity(new Vec3(moveDir.x, velocity.y, moveDir.z));

            // Xoay theo hướng camera
            const angle = Math.atan2(-camForward.x, -camForward.z) * 180 / Math.PI;
            this.node.eulerAngles = new Vec3(0, angle, 0);

            this.playAnim('walk');
        } else {
            // Dừng lại -> giữ lại velocity.y, set xz về 0
            const velocity = new Vec3();
            this._rigidBody.getLinearVelocity(velocity);
            this._rigidBody.setLinearVelocity(new Vec3(0, velocity.y, 0));
            this.playAnim('idle');
        }

        // Kiểm tra đang rơi
        const velocity = new Vec3();
        this._rigidBody.getLinearVelocity(velocity);
        if (velocity.y < -0.1 && !this._canJump) {
            this.playAnim('fall');
        }
    }

    public jump() {
        if (!this._canJump) return;

        console.log('jump');
        const velocity = new Vec3(0, 0, 0);

        if (!this._rigidBody) {
            console.warn("🔥 RigidBody chưa được gán!");

        }
        this._rigidBody.getLinearVelocity(velocity);
        
        velocity.y += this.jumpForce;
        console.log(velocity.y);

        this._rigidBody.setLinearVelocity(velocity);

        this._canJump = false;
        this.playAnim('jump');

        // Tạm set nhảy lại sau 0.6s
        setTimeout(() => {
            this._canJump = true;
        }, 600);
    }

    private playAnim(name: string) {
        if (!this._animation || this._currentAnim === name) return;
        this._currentAnim = name;
        this._animation.play(name);
    }
}
