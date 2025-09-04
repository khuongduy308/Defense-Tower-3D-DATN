import { _decorator, Component, Node, Prefab, instantiate, Vec3 } from 'cc';
import { BaseEnemy } from './Enemy/BaseEnemy';

const { ccclass, property } = _decorator;

type GridTile = {
    node: Node;
    x: number;
    z: number;
};

type EnemyConfig = {
    prefab: Prefab;
    count: number;
    delay: number; // delay giữa từng enemy
};

type WaveConfig = {
    enemies: EnemyConfig[];
    delayAfterWave: number; // delay trước khi bắt đầu wave tiếp theo
};

@ccclass('GameManager')
export class GameManager extends Component {
    @property({ type: Prefab }) baseEnemyPrefab: Prefab = null;
    @property({ type: Prefab }) fastEnemyPrefab: Prefab = null;

    @property({ type: Node }) mapNode: Node = null;
    @property({ type: Node }) enemiesContainer: Node = null;
    @property({ type: Node }) startPoint: Node = null;
    @property({ type: Node }) endPoint: Node = null;

    
    // // ✅ Cấu hình wave
    // @property
    // totalWaves: number = 3; // tổng số wave

    // @property
    // enemiesPerWave: number = 5; // số quái mỗi wave

    // @property
    // delayBetweenEnemies: number = 1; // delay giữa từng quái trong 1 wave (giây)

    // @property
    // delayBetweenWaves: number = 5; // delay giữa các wave (giây)

    private path: Vec3[] = [];
    private currentWave: number = 0;
    private spawnEnemySchedule = () => { }; // placeholder

    private wavesConfig: WaveConfig[] = [];

    start() {
        this.scheduleOnce(() => {
            this.extractPathFromMap();

            this.wavesConfig = [
                {
                    enemies: [
                        { prefab: this.baseEnemyPrefab, count: 5, delay: 1 },
                        { prefab: this.fastEnemyPrefab, count: 2, delay: 2 }
                    ],
                    delayAfterWave: 5
                },
                {
                    enemies: [
                        { prefab: this.baseEnemyPrefab, count: 8, delay: 0.8 },
                        { prefab: this.fastEnemyPrefab, count: 1, delay: 3 }
                    ],
                    delayAfterWave: 8
                }
            ];

            this.startWaveSystem();
        }, 0.1);
    }

    startWaveSystem() {
        this.currentWave = 0;
        this.spawnNextWave();
    }

    spawnNextWave() {
        if (this.currentWave >= this.wavesConfig.length) {
            console.log("🎉 Hoàn thành tất cả wave!");
            return;
        }

        const wave = this.wavesConfig[this.currentWave];
        this.currentWave++;
        console.log(`🚀 Bắt đầu wave ${this.currentWave}/${this.wavesConfig.length}`);

        let enemyIndex = 0;

        const spawnEnemyGroup = () => {
            if (enemyIndex >= wave.enemies.length) {
                // wave xong → đợi rồi qua wave tiếp theo
                this.scheduleOnce(() => {
                    this.spawnNextWave();
                }, wave.delayAfterWave);
                return;
            }

            const group = wave.enemies[enemyIndex];
            let spawned = 0;

            this.schedule(() => {
                if (spawned >= group.count) {
                    this.unscheduleAllCallbacks();
                    enemyIndex++;
                    // spawn nhóm kế tiếp
                    this.scheduleOnce(spawnEnemyGroup, 0.5);
                    return;
                }
                this.spawnEnemy(group.prefab);
                spawned++;
            }, group.delay, group.count - 1);
        };

        spawnEnemyGroup();
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

    spawnEnemy(prefab: Prefab) {
        if (this.path.length === 0) {
            console.warn("⚠️ No path found for enemy!");
            return;
        }

        const enemy = instantiate(prefab);
        enemy.setParent(this.enemiesContainer);
        enemy.setWorldPosition(this.path[0]);

        const script = enemy.getComponent(BaseEnemy);
        if (script) {
            script.init(this.path);
        }
    }
}
