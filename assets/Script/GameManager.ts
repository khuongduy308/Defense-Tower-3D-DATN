import { _decorator, Component, Node, Prefab, instantiate, Vec3 } from 'cc';
import { BaseEnemy } from './Enemy/BaseEnemy';

const { ccclass, property } = _decorator;

type GridTile = {
    node: Node;
    x: number;
    z: number;
};

@ccclass('GameManager')
export class GameManager extends Component {
    @property({ type: Prefab }) enemyPrefab: Prefab = null;
    @property({ type: Node }) mapNode: Node = null;
    @property({ type: Node }) enemiesContainer: Node = null;
    @property({ type: Node }) startPoint: Node = null;
    @property({ type: Node }) endPoint: Node = null;


    @property
    moveSpeed: number = 2;

    private path: Vec3[] = [];

    start() {
        this.scheduleOnce(() => {
            this.extractPathFromMap();
            this.spawnWave(5, 1);
        }, 0.1); // Delay 1 frame (bảo đảm map đã render xong)
    }


    extractPathFromMap() {
        const grid: Record<string, GridTile> = {};
        let startPos: Vec3 = this.startPoint.getWorldPosition();
        let endPos: Vec3 = this.endPoint.getWorldPosition();

        for (const tile of this.mapNode.children) {
            const name = tile.name;
            const pos = tile.getPosition();
            const x = Math.round(pos.x);
            const z = Math.round(pos.z);

            if (name.includes("Path") || name.includes("Start") || name.includes("End")) {
                grid[`${x},${z}`] = { node: tile, x, z };
            }
        }

        if (!startPos || !endPos) {
            console.error("❌ Không tìm thấy điểm Start hoặc End");
            return;
        }

        // ✅ Duyệt đường đi BFS
        const queue: { x: number, z: number, path: Vec3[] }[] = [];
        const visited = new Set<string>();

        const sx = Math.round(startPos.x);
        const sz = Math.round(startPos.z);
        const ex = Math.round(endPos.x);
        const ez = Math.round(endPos.z);

        queue.push({ x: sx, z: sz, path: [startPos.clone()] });
        visited.add(`${sx},${sz}`);

        const directions = [
            { dx: 1, dz: 0 },  // →
            { dx: 0, dz: 1 },  // ↓
            { dx: 0, dz: -1 }, // ↑
        ];

        while (queue.length > 0) {
            const { x, z, path } = queue.shift();
            if (x === ex && z === ez) {
                this.path = path;
                console.log("✅ Đường đi tìm được:", path);
                return;
            }

            for (const { dx, dz } of directions) {
                const nx = x + dx;
                const nz = z + dz;
                const key = `${nx},${nz}`;
                if (grid[key] && !visited.has(key)) {
                    visited.add(key);
                    queue.push({
                        x: nx,
                        z: nz,
                        path: [...path, grid[key].node.getWorldPosition()]
                    });
                }
            }
        }

        console.warn("⚠️ Không tìm được đường đi từ Start đến End.");
    }


    spawnWave(count: number, delay: number = 1) {
        let spawned = 0;
        this.schedule(() => {
            if (spawned >= count) {
                this.unscheduleAllCallbacks();
                return;
            }
            this.spawnEnemy();
            spawned++;
        }, delay);
    }

    spawnEnemy() {
        if (this.path.length === 0) {
            console.warn("⚠️ No path found for enemy!");
            return;
        }

        const enemy = instantiate(this.enemyPrefab);
        enemy.setParent(this.enemiesContainer);
        enemy.setWorldPosition(this.path[0]);

        const script = enemy.getComponent(BaseEnemy);
        if (script) {
            script.moveSpeed = this.moveSpeed;
            script.init(this.path);
        }
    }
}
