const container = document.getElementById('canvas-container');

const scene = new THREE.Scene();
scene.background = new THREE.Color(0x181824);
scene.fog = new THREE.FogExp2(0x181824, 0.12);

const initialWidth = container ? container.clientWidth : window.innerWidth;
const initialHeight = container ? container.clientHeight : window.innerHeight;

const camera = new THREE.PerspectiveCamera(45, initialWidth / initialHeight, 0.1, 100);
camera.position.set(4.5, 8.5, 10.0);

const renderer = new THREE.WebGLRenderer({ antialias: true });
renderer.setSize(initialWidth, initialHeight);
renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2));
renderer.shadowMap.enabled = true;
renderer.shadowMap.type = THREE.PCFSoftShadowMap;

if (container) {
    container.appendChild(renderer.domElement);
} else {
    document.body.appendChild(renderer.domElement);
}

const controls = new THREE.OrbitControls(camera, renderer.domElement);
controls.enableDamping = true;
controls.target.set(0, 0.5, 0.0); 
controls.maxPolarAngle = Math.PI / 2 - 0.05;

controls.minDistance = 1.6; 
controls.maxDistance = 6.0; 

controls.enabled = false; 

const skinMat = new THREE.MeshStandardMaterial({ color: 0xffd1b3, roughness: 0.95, metalness: 0.0 });
const hairMat = new THREE.MeshStandardMaterial({ color: 0x1f1f23, roughness: 0.95, metalness: 0.0 });
const blueMat = new THREE.MeshStandardMaterial({ color: 0x337acc, roughness: 0.90, metalness: 0.0 });
const pinkMat = new THREE.MeshStandardMaterial({ color: 0xffa3b8, roughness: 0.90, metalness: 0.0 }); 
const coralMat = new THREE.MeshStandardMaterial({ color: 0xff7b7b, roughness: 0.85, metalness: 0.1 }); 
const metalMat = new THREE.MeshStandardMaterial({ color: 0xcccccc, metalness: 0.8, roughness: 0.2 });
const darkMetalMat = new THREE.MeshStandardMaterial({ color: 0x222222, metalness: 0.7, roughness: 0.4 });
const whiteMat = new THREE.MeshStandardMaterial({ color: 0xeeeeee, roughness: 0.85, metalness: 0.0 }); 
const mugMat = new THREE.MeshStandardMaterial({ color: 0xf5f5f5, roughness: 0.3 });
const wallMat = new THREE.MeshStandardMaterial({ color: 0x3b2a20, roughness: 0.9, metalness: 0.0 });
const woodMat = new THREE.MeshStandardMaterial({ color: 0x2f1f17, roughness: 0.7, metalness: 0.1 });

const canvas = document.createElement('canvas');
canvas.width = 512;
canvas.height = 384;
const ctx = canvas.getContext('2d');
const screenTexture = new THREE.CanvasTexture(canvas);

function drawCodeScreen(cursorBlink) {
    ctx.fillStyle = '#1e1e1e';
    ctx.fillRect(0, 0, canvas.width, canvas.height);

    ctx.fillStyle = '#252526';
    ctx.fillRect(0, 0, 40, canvas.height);
    ctx.fillRect(40, 0, canvas.width, 30);
    
    ctx.fillStyle = '#858585';
    ctx.font = '12px Courier New';
    ctx.fillText('index.js', 50, 20);

    const lines = [
        { indent: 0, blocks: [{ w: 60, c: '#569cd6' }, { w: 90, c: '#4fc1ff' }, { w: 120, c: '#9cdcfe' }] },
        { indent: 1, blocks: [{ w: 80, c: '#ce9178' }, { w: 40, c: '#b5cea8' }] },
        { indent: 2, blocks: [{ w: 110, c: '#6a9955' }] }, 
        { indent: 1, blocks: [{ w: 50, c: '#c586c0' }, { w: 70, c: '#dcdcaa' }] },
        { indent: 2, blocks: [{ w: 130, c: '#4fc1ff' }, { w: 50, c: '#9cdcfe' }] },
        { indent: 2, blocks: [{ w: 90, c: '#569cd6' }] },
        { indent: 1, blocks: [{ w: 40, c: '#c586c0' }] },
        { indent: 0, blocks: [{ w: 30, c: '#c586c0' }] }
    ];

    let startY = 60;
    lines.forEach((line) => {
        let currentX = 60 + (line.indent * 25);
        line.blocks.forEach(block => {
            ctx.fillStyle = block.c;
            ctx.fillRect(currentX, startY, block.w, 10);
            currentX += block.w + 10;
        });
        startY += 24;
    });

    if (cursorBlink) {
        ctx.fillStyle = '#ffffff';
        ctx.fillRect(210, 204, 8, 14);
    }
    screenTexture.needsUpdate = true;
}

function createBookSpineTexture(title, bgColor, textColor) {
    const bookCanvas = document.createElement('canvas');
    bookCanvas.width = 256;
    bookCanvas.height = 64;
    const bCtx = bookCanvas.getContext('2d');
    bCtx.fillStyle = bgColor;
    bCtx.fillRect(0, 0, bookCanvas.width, bookCanvas.height);
    bCtx.fillStyle = textColor;
    bCtx.font = 'bold 22px Arial';
    bCtx.textAlign = 'center';
    bCtx.textBaseline = 'middle';
    bCtx.fillText(title, bookCanvas.width / 2, bookCanvas.height / 2);
    const tex = new THREE.CanvasTexture(bookCanvas);
    return tex;
}

const ambientLight = new THREE.HemisphereLight(0xffe2cc, 0x222235, 0.65); 
scene.add(ambientLight);

const keyLight = new THREE.DirectionalLight(0xffdfb3, 0.65); 
keyLight.position.set(5, 5, 4);
keyLight.castShadow = true;
keyLight.shadow.mapSize.set(1024, 1024);
scene.add(keyLight);

const fillLight = new THREE.DirectionalLight(0xa6c8ff, 0.35); 
fillLight.position.set(-5, 3, 2);
scene.add(fillLight);

const rimLight = new THREE.DirectionalLight(0xffffff, 0.35); 
rimLight.position.set(0, 4, -4);
scene.add(rimLight);

const deskSpot = new THREE.SpotLight(0xffe0b3, 1.4, 8, Math.PI / 4, 0.5, 1);
deskSpot.position.set(-0.6, 1.2, 0.5);
deskSpot.target.position.set(0, 0, 0.5);
deskSpot.castShadow = true;
scene.add(deskSpot);
scene.add(deskSpot.target);

const screenGlow = new THREE.PointLight(0x3399ff, 1.1, 2);
screenGlow.position.set(0, 0.15, 0.4);
scene.add(screenGlow);

const floorGeo = new THREE.PlaneGeometry(10, 10);
const floor = new THREE.Mesh(floorGeo, woodMat);
floor.rotation.x = -Math.PI / 2;
floor.position.y = -0.7;
floor.receiveShadow = true;
scene.add(floor);

const wallL = new THREE.Mesh(new THREE.PlaneGeometry(10, 6), wallMat);
wallL.position.set(-3, 2.3, 0);
wallL.rotation.y = Math.PI / 2;
wallL.receiveShadow = true;
scene.add(wallL);

const wallB = new THREE.Mesh(new THREE.PlaneGeometry(10, 6), wallMat);
wallB.position.set(0, 2.3, -3);
wallB.receiveShadow = true;
scene.add(wallB);

const desk = new THREE.Mesh(new THREE.BoxGeometry(2.2, 0.06, 1.0), new THREE.MeshStandardMaterial({ color: 0x1e1e1e, roughness: 0.5 }));
desk.position.set(0, -0.15, 0.65); 
desk.receiveShadow = true;
desk.castShadow = true;
scene.add(desk);

const legGeo = new THREE.CylinderGeometry(0.03, 0.03, 0.55);
const legLocations = [ [-1.0, -0.425, 0.2], [1.0, -0.425, 0.2], [-1.0, -0.425, 1.1], [1.0, -0.425, 1.1] ];
legLocations.forEach(pos => {
    const leg = new THREE.Mesh(legGeo, darkMetalMat);
    leg.position.set(pos[0], pos[1], pos[2]);
    leg.castShadow = true;
    scene.add(leg);
});

const mugGroup = new THREE.Group();
mugGroup.position.set(-0.55, -0.04, 0.6); 
const mugBody = new THREE.Mesh(new THREE.CylinderGeometry(0.06, 0.06, 0.15, 24), mugMat);
const mugHandle = new THREE.Mesh(new THREE.TorusGeometry(0.04, 0.012, 8, 24, Math.PI), mugMat);
mugHandle.position.set(-0.06, 0, 0);
mugHandle.rotation.z = Math.PI / 2;
mugGroup.add(mugBody);
mugGroup.add(mugHandle);

const teaMat = new THREE.MeshStandardMaterial({ color: 0x4a2511, roughness: 0.1, metalness: 0.1 });
const teaGeo = new THREE.CylinderGeometry(0.054, 0.054, 0.01, 24);
const tea = new THREE.Mesh(teaGeo, teaMat);
tea.position.y = 0.06; 
mugGroup.add(tea);

scene.add(mugGroup);

const laptopGroup = new THREE.Group();
laptopGroup.position.set(0, -0.12, 0.65); 

const laptopBase = new THREE.Mesh(new THREE.BoxGeometry(0.7, 0.02, 0.5), metalMat);
laptopBase.castShadow = true;
laptopGroup.add(laptopBase);

const laptopHinge = new THREE.Mesh(new THREE.CylinderGeometry(0.012, 0.012, 0.68, 16), darkMetalMat);
laptopHinge.rotation.z = Math.PI / 2;
laptopHinge.position.set(0, 0.01, 0.24);
laptopGroup.add(laptopHinge);

const trackpad = new THREE.Mesh(new THREE.BoxGeometry(0.18, 0.002, 0.11), darkMetalMat);
trackpad.position.set(0, 0.011, -0.16);
laptopGroup.add(trackpad);

const keysGroup = new THREE.Group();
const keyGeo = new THREE.BoxGeometry(0.032, 0.008, 0.025);
const keyRows = 5;
const keyCols = 12;
for (let r = 0; r < keyRows; r++) {
    for (let c = 0; c < keyCols; c++) {
        const key = new THREE.Mesh(keyGeo, darkMetalMat);
        key.position.set(
            -0.25 + (c * 0.046) + (r % 2 * 0.01),
            0.011,
            -0.08 + (r * 0.045)
        );
        keysGroup.add(key);
    }
}
laptopGroup.add(keysGroup);

const laptopLidPivot = new THREE.Group();
laptopLidPivot.position.set(0, 0.01, 0.24);

const laptopScreenBody = new THREE.Mesh(new THREE.BoxGeometry(0.7, 0.48, 0.02), metalMat);
laptopScreenBody.position.set(0, 0.24, 0); 
laptopScreenBody.castShadow = true;

const laptopScreenDisplay = new THREE.Mesh(
    new THREE.PlaneGeometry(0.66, 0.44), 
    new THREE.MeshBasicMaterial({ map: screenTexture, side: THREE.DoubleSide })
);
laptopScreenDisplay.position.set(0, 0.24, -0.011);

laptopLidPivot.add(laptopScreenBody);
laptopLidPivot.add(laptopScreenDisplay);

laptopLidPivot.rotation.x = 0.45; 
laptopGroup.add(laptopLidPivot);

scene.add(laptopGroup);

function createBook(title, coverColorHex, textColorHex) {
    const bookGroup = new THREE.Group();
    const w = 0.34;
    const h = 0.045;
    const d = 0.25;
    const bookGeo = new THREE.BoxGeometry(w, h, d);
    
    const pageColor = new THREE.MeshStandardMaterial({ color: 0xf5f2eb, roughness: 0.9 });
    const coverMat = new THREE.MeshStandardMaterial({ color: coverColorHex, roughness: 0.95 });
    
    const spineTex = createBookSpineTexture(title, coverColorHex, textColorHex);
    const spineMat = new THREE.MeshStandardMaterial({ map: spineTex, roughness: 0.95 });
    
    const materials = [
        pageColor,  
        pageColor,  
        coverMat,   
        coverMat,   
        spineMat,   
        pageColor   
    ];
    
    const bookMesh = new THREE.Mesh(bookGeo, materials);
    bookMesh.castShadow = true;
    bookMesh.receiveShadow = true;
    bookGroup.add(bookMesh);
    return bookGroup;
}

const bookStack = new THREE.Group();
bookStack.position.set(0.65, -0.09, 0.65); 

const b1 = createBook("Clean Code", "#2e7d32", "#ffffff");
b1.position.y = 0;
b1.rotation.y = 0.15; 

const b2 = createBook("Mathematics", "#1a237e", "#ffd700");
b2.position.y = 0.045;
b2.rotation.y = -0.1;

const b3 = createBook("Refactoring", "#b71c1c", "#ffffff");
b3.position.y = 0.09;
b3.rotation.y = 0.05;

const b4 = createBook("Algorithms", "#4a148c", "#ffee58");
b4.position.y = 0.135;
b4.rotation.y = -0.2;

bookStack.add(b1);
bookStack.add(b2);
bookStack.add(b3);
bookStack.add(b4);
scene.add(bookStack);

const pencil1 = new THREE.Group();
pencil1.position.set(0.46, -0.11, 0.75); 
pencil1.rotation.set(0.1, 0.4, 0.1);

const pBody1 = new THREE.Mesh(new THREE.CylinderGeometry(0.008, 0.008, 0.12, 6), new THREE.MeshStandardMaterial({ color: 0xffd54f, roughness: 0.8 }));
pBody1.rotation.x = Math.PI / 2;
pencil1.add(pBody1);

const pTip1 = new THREE.Mesh(new THREE.ConeGeometry(0.008, 0.025, 6), new THREE.MeshStandardMaterial({ color: 0xffe0b2, roughness: 0.9 }));
pTip1.position.set(0, 0, 0.0725);
pTip1.rotation.x = Math.PI / 2;
pencil1.add(pTip1);

const pLead1 = new THREE.Mesh(new THREE.ConeGeometry(0.003, 0.008, 6), new THREE.MeshStandardMaterial({ color: 0x1f1f23, roughness: 0.9 }));
pLead1.position.set(0, 0, 0.081);
pLead1.rotation.x = Math.PI / 2;
pencil1.add(pLead1);

const pEraser1 = new THREE.Mesh(new THREE.CylinderGeometry(0.008, 0.008, 0.015, 6), new THREE.MeshStandardMaterial({ color: 0xff8a80, roughness: 0.8 }));
pEraser1.position.set(0, 0, -0.0675);
pEraser1.rotation.x = Math.PI / 2;
pencil1.add(pEraser1);
scene.add(pencil1);

const pencil2 = new THREE.Group();
pencil2.position.set(0.48, -0.11, 0.72);
pencil2.rotation.set(-0.1, 0.25, -0.1);

const pBody2 = new THREE.Mesh(new THREE.CylinderGeometry(0.008, 0.008, 0.12, 6), new THREE.MeshStandardMaterial({ color: 0x26a69a, roughness: 0.8 }));
pBody2.rotation.x = Math.PI / 2;
pencil2.add(pBody2);

const pTip2 = new THREE.Mesh(new THREE.ConeGeometry(0.008, 0.025, 6), new THREE.MeshStandardMaterial({ color: 0xffe0b2, roughness: 0.9 }));
pTip2.position.set(0, 0, 0.0725);
pTip2.rotation.x = Math.PI / 2;
pencil2.add(pTip2);

const pLead2 = new THREE.Mesh(new THREE.ConeGeometry(0.003, 0.008, 6), new THREE.MeshStandardMaterial({ color: 0x1f1f23, roughness: 0.9 }));
pLead2.position.set(0, 0, 0.081);
pLead2.rotation.x = Math.PI / 2;
pencil2.add(pLead2);

const pEraser2 = new THREE.Mesh(new THREE.CylinderGeometry(0.008, 0.008, 0.015, 6), new THREE.MeshStandardMaterial({ color: 0xff8a80, roughness: 0.8 }));
pEraser2.position.set(0, 0, -0.0675);
pEraser2.rotation.x = Math.PI / 2;
pencil2.add(pEraser2);
scene.add(pencil2);

const eraser = new THREE.Mesh(
    new THREE.BoxGeometry(0.035, 0.015, 0.02),
    new THREE.MeshStandardMaterial({ color: 0xff8a80, roughness: 0.9 })
);
eraser.position.set(0.42, -0.11, 0.68);
eraser.rotation.y = 0.55;
eraser.castShadow = true;
scene.add(eraser);

const chairGroup = new THREE.Group();
chairGroup.position.set(0, -0.65, -0.03); 

const starBase = new THREE.Group();
for (let i = 0; i < 5; i++) {
    const legPivot = new THREE.Group();
    const angle = (i * Math.PI * 2) / 5;
    legPivot.rotation.y = angle;

    const leg = new THREE.Mesh(new THREE.CylinderGeometry(0.02, 0.02, 0.35), darkMetalMat);
    leg.rotation.x = Math.PI / 2;
    leg.position.set(0, 0.08, 0.175);
    legPivot.add(leg);

    const wheel = new THREE.Mesh(new THREE.CylinderGeometry(0.025, 0.025, 0.02), darkMetalMat);
    wheel.rotation.x = Math.PI / 2;
    wheel.position.set(0, 0.025, 0.35);
    legPivot.add(wheel);

    starBase.add(legPivot);
}
chairGroup.add(starBase);

const supportPole = new THREE.Mesh(new THREE.CylinderGeometry(0.035, 0.035, 0.25), metalMat);
supportPole.position.y = 0.2;
chairGroup.add(supportPole);

const seatMesh = new THREE.Mesh(new THREE.BoxGeometry(0.5, 0.08, 0.5), new THREE.MeshStandardMaterial({ color: 0x222228, roughness: 0.8 }));
seatMesh.position.y = 0.325;
seatMesh.castShadow = true;
chairGroup.add(seatMesh);

const bracket = new THREE.Mesh(new THREE.BoxGeometry(0.08, 0.4, 0.05), darkMetalMat);
bracket.position.set(0, 0.55, -0.22);
bracket.rotation.x = -0.15;
chairGroup.add(bracket);

const backrest = new THREE.Mesh(new THREE.BoxGeometry(0.42, 0.38, 0.06), new THREE.MeshStandardMaterial({ color: 0x222228, roughness: 0.8 }));
backrest.position.set(0, 0.72, -0.26);
backrest.castShadow = true;
chairGroup.add(backrest);

scene.add(chairGroup);

const charGroup = new THREE.Group();
charGroup.position.set(0, -0.32, 0.02);

const hips = new THREE.Group();
charGroup.add(hips);

const torso = new THREE.Mesh(new THREE.CylinderGeometry(0.15, 0.19, 0.58, 32), blueMat);
torso.position.y = 0.29; 
torso.castShadow = true;
hips.add(torso);

const neck = new THREE.Mesh(new THREE.CylinderGeometry(0.08, 0.1, 0.18, 32), skinMat);
neck.position.y = 0.66; 
torso.add(neck);

const headGroup = new THREE.Group();
headGroup.position.set(0, 0.73, 0.02); 

const headGeo = new THREE.SphereGeometry(0.40, 48, 48);
headGeo.scale(1.0, 1.04, 0.95); 
const head = new THREE.Mesh(headGeo, skinMat);
head.castShadow = true;
headGroup.add(head);

const noseGeo = new THREE.SphereGeometry(0.04, 16, 16);
noseGeo.scale(1, 1.4, 1.25);
const nose = new THREE.Mesh(noseGeo, skinMat);
nose.position.set(0, -0.05, 0.38);
headGroup.add(nose);

const hairGroup = new THREE.Group();

const hairCapGeo = new THREE.SphereGeometry(0.406, 48, 48);
hairCapGeo.scale(1.015, 1.055, 0.965); 
const hairCap = new THREE.Mesh(hairCapGeo, hairMat);
hairCap.position.set(0, 0.015, -0.03); 
hairCap.castShadow = true;
hairCap.receiveShadow = true;
hairGroup.add(hairCap);

function createClayLock(rx, ry, rz, sx, sy, sz, px, py, pz) {
    const lockGeo = new THREE.SphereGeometry(1, 32, 32);
    lockGeo.scale(sx, sy, sz);
    const lockMesh = new THREE.Mesh(lockGeo, hairMat);
    lockMesh.position.set(px, py, pz);
    lockMesh.rotation.set(rx, ry, rz);
    lockMesh.castShadow = true;
    lockMesh.receiveShadow = true;
    return lockMesh;
}

hairGroup.add(createClayLock(-0.1, 0, 0,  0.38, 0.48, 0.34,  0, -0.1, -0.22));
hairGroup.add(createClayLock(-0.1, 0.3, -0.1,  0.3, 0.45, 0.28,  -0.2, -0.15, -0.18));
hairGroup.add(createClayLock(-0.1, -0.3, 0.1,  0.3, 0.45, 0.28,  0.2, -0.15, -0.18));

hairGroup.add(createClayLock(0.1, 0.2, -0.1,  0.14, 0.38, 0.14,  -0.35, -0.15, 0.14));
hairGroup.add(createClayLock(0.0, 0.3, -0.05,  0.12, 0.32, 0.12,  -0.38, -0.12, -0.02));
hairGroup.add(createClayLock(0.1, -0.2, 0.1,  0.14, 0.38, 0.14,  0.35, -0.15, 0.14));
hairGroup.add(createClayLock(0.0, -0.3, 0.05,  0.12, 0.32, 0.12,  0.38, -0.12, -0.02));

hairGroup.add(createClayLock(0.3, 0.4, -0.4,  0.18, 0.24, 0.11,  -0.16, 0.20, 0.28));
hairGroup.add(createClayLock(0.2, 0.6, -0.6,  0.14, 0.20, 0.10,  -0.28, 0.16, 0.22));
hairGroup.add(createClayLock(0.3, -0.4, 0.4,  0.18, 0.24, 0.11,  0.16, 0.20, 0.28));
hairGroup.add(createClayLock(0.2, -0.6, 0.6,  0.14, 0.20, 0.10,  0.28, 0.16, 0.22));
hairGroup.add(createClayLock(0.2, 0, -0.1,  0.08, 0.15, 0.07,  -0.02, 0.23, 0.32));

hairGroup.add(createClayLock(0.2, 0, 0.2,  0.16, 0.16, 0.16,  -0.28, 0.36, -0.12));
hairGroup.add(createClayLock(0.2, 0, -0.2,  0.16, 0.16, 0.16,  0.28, 0.36, -0.12));

const tieGeo = new THREE.TorusGeometry(0.10, 0.022, 10, 32);
const tieL = new THREE.Mesh(tieGeo, coralMat);
tieL.position.set(-0.27, 0.29, -0.11);
tieL.rotation.set(0.6, 0, 0.2);
tieL.castShadow = true;
hairGroup.add(tieL);

const tieR = new THREE.Mesh(tieGeo, coralMat);
tieR.position.set(0.27, 0.29, -0.11);
tieR.rotation.set(0.6, 0, -0.2);
tieR.castShadow = true;
hairGroup.add(tieR);

headGroup.add(hairGroup);

const headphonesGroup = new THREE.Group();
headphonesGroup.position.set(0, 0.02, 0.0); 

const earCupLGeo = new THREE.SphereGeometry(0.12, 32, 32);
earCupLGeo.scale(0.6, 1.25, 1.25); 
const earCupL = new THREE.Mesh(earCupLGeo, pinkMat); 
earCupL.position.set(-0.41, 0.0, 0.02);
earCupL.rotation.y = 0.1; 
earCupL.castShadow = true;
headphonesGroup.add(earCupL);

const earCupRGeo = new THREE.SphereGeometry(0.12, 32, 32);
earCupRGeo.scale(0.6, 1.25, 1.25);
const earCupR = new THREE.Mesh(earCupRGeo, pinkMat); 
earCupR.position.set(0.41, 0.0, 0.02);
earCupR.rotation.y = -0.1;
earCupR.castShadow = true;
headphonesGroup.add(earCupR);

const headphoneBandGeo = new THREE.TorusGeometry(0.43, 0.035, 16, 64, Math.PI);
const headphoneBand = new THREE.Mesh(headphoneBandGeo, pinkMat); 
headphoneBand.position.set(0, 0.0, 0.02);
headphoneBand.castShadow = true;
headphonesGroup.add(headphoneBand);

const pinGeo = new THREE.CylinderGeometry(0.015, 0.015, 0.06, 16);
const pinL = new THREE.Mesh(pinGeo, metalMat);
pinL.position.set(-0.41, 0.1, 0.02);
pinL.rotation.z = Math.PI / 2;
const pinR = pinL.clone();
pinR.position.x = 0.41;
headphonesGroup.add(pinL);
headphonesGroup.add(pinR);
headGroup.add(headphonesGroup);

const eyeGeo = new THREE.SphereGeometry(0.032, 24, 24);
const eyeL = new THREE.Mesh(eyeGeo, hairMat); eyeL.position.set(-0.14, 0.02, 0.37);
const eyeR = eyeL.clone(); eyeR.position.x = 0.14;
headGroup.add(eyeL);
headGroup.add(eyeR);

hips.add(headGroup);

const thighL = new THREE.Mesh(new THREE.CylinderGeometry(0.1, 0.07, 0.55, 32), blueMat);
thighL.rotation.x = Math.PI / 2;
thighL.position.set(-0.11, 0.08, 0.26);
thighL.castShadow = true;
const thighR = thighL.clone(); thighR.position.x = 0.11;
hips.add(thighL);
hips.add(thighR);

const shinL = new THREE.Mesh(new THREE.CylinderGeometry(0.07, 0.05, 0.52, 32), blueMat);
shinL.position.set(-0.11, -0.18, 0.52);
shinL.rotation.x = 0.1;
shinL.castShadow = true;
const shinR = shinL.clone(); shinR.position.x = 0.11;
hips.add(shinL);
hips.add(shinR);

const shoeGeo = new THREE.SphereGeometry(0.085, 24, 24);
shoeGeo.scale(1.1, 0.7, 1.8);

const footL = new THREE.Mesh(shoeGeo, whiteMat); 
footL.position.set(-0.11, -0.42, 0.56);
footL.castShadow = true;
const footR = footL.clone(); footR.position.x = 0.11;
hips.add(footL);
hips.add(footR);

const shoulderRelativeY = 0.10; 

const leftShoulder = new THREE.Group();
leftShoulder.position.set(-0.16, shoulderRelativeY, 0.08); 
torso.add(leftShoulder);

const upperArmL = new THREE.Mesh(new THREE.CylinderGeometry(0.06, 0.048, 0.21, 24), blueMat);
upperArmL.position.y = -0.105;
upperArmL.castShadow = true;
leftShoulder.add(upperArmL);

const shoulderJointL = new THREE.Mesh(new THREE.SphereGeometry(0.06, 24, 24), blueMat);
leftShoulder.add(shoulderJointL);

const leftElbow = new THREE.Group();
leftElbow.position.set(0, -0.21, 0); 
leftShoulder.add(leftElbow);

const forearmL = new THREE.Mesh(new THREE.CylinderGeometry(0.045, 0.035, 0.23, 24), skinMat); 
forearmL.position.y = -0.115;
forearmL.castShadow = true;
leftElbow.add(forearmL);

const elbowJointL = new THREE.Mesh(new THREE.SphereGeometry(0.046, 24, 24), skinMat);
leftElbow.add(elbowJointL);

const leftWrist = new THREE.Group();
leftWrist.position.set(0, -0.23, 0);
leftElbow.add(leftWrist);

const palmLGeo = new THREE.SphereGeometry(0.045, 24, 24);
palmLGeo.scale(1.2, 0.65, 1.3);
const handL = new THREE.Mesh(palmLGeo, skinMat);
handL.position.set(0.015, -0.015, 0.025);
handL.castShadow = true;
leftWrist.add(handL);

const fingerRadius = 0.010;
for (let f = 0; f < 4; f++) {
    const finger = new THREE.Group();
    handL.add(finger);

    const splayX = -0.020 + (f * 0.013); 
    
    const p0 = new THREE.Vector3(splayX, -0.002, 0.03); 
    const p1 = new THREE.Vector3(splayX * 1.1, 0.006, 0.055); 
    const p2 = new THREE.Vector3(splayX * 1.2, -0.026, 0.075); 

    const curve = new THREE.CatmullRomCurve3([p0, p1, p2]);
    const tubeGeo = new THREE.TubeGeometry(curve, 16, fingerRadius, 12, false);
    const tube = new THREE.Mesh(tubeGeo, skinMat);
    tube.castShadow = true;
    finger.add(tube);

    const tipGeo = new THREE.SphereGeometry(fingerRadius, 12, 12);
    const tip = new THREE.Mesh(tipGeo, skinMat);
    tip.position.copy(p2);
    tip.castShadow = true;
    finger.add(tip);

    const baseGeo = new THREE.SphereGeometry(fingerRadius * 1.05, 12, 12);
    const base = new THREE.Mesh(baseGeo, skinMat);
    base.position.copy(p0);
    base.castShadow = true;
    finger.add(base);
}

const thumbL = new THREE.Group();
thumbL.position.set(0.015, -0.005, 0.015); 
handL.add(thumbL);

const tp0L = new THREE.Vector3(0, 0, 0);
const tp1L = new THREE.Vector3(0.015, -0.002, 0.015);
const tp2L = new THREE.Vector3(0.022, -0.012, 0.025); 

const thumbCurveL = new THREE.CatmullRomCurve3([tp0L, tp1L, tp2L]);
const thumbRadius = 0.011; 
const thumbMeshL = new THREE.Mesh(new THREE.TubeGeometry(thumbCurveL, 12, thumbRadius, 10, false), skinMat);
thumbMeshL.castShadow = true;
thumbL.add(thumbMeshL);

const thumbTipL = new THREE.Mesh(new THREE.SphereGeometry(thumbRadius, 10, 10), skinMat);
thumbTipL.position.copy(tp2L);
thumbTipL.castShadow = true;
thumbL.add(thumbTipL);

const wristJointL = new THREE.Mesh(new THREE.SphereGeometry(0.036, 24, 24), skinMat);
leftWrist.add(wristJointL);

const rightShoulder = new THREE.Group();
rightShoulder.position.set(0.16, shoulderRelativeY, 0.08); 
torso.add(rightShoulder);

const upperArmR = new THREE.Mesh(new THREE.CylinderGeometry(0.06, 0.048, 0.21, 24), blueMat);
upperArmR.position.y = -0.105;
upperArmR.castShadow = true;
rightShoulder.add(upperArmR);

const shoulderJointR = new THREE.Mesh(new THREE.SphereGeometry(0.06, 24, 24), blueMat);
rightShoulder.add(shoulderJointR);

const rightElbow = new THREE.Group();
rightElbow.position.set(0, -0.21, 0); 
rightShoulder.add(rightElbow);

const forearmR = new THREE.Mesh(new THREE.CylinderGeometry(0.045, 0.035, 0.23, 24), skinMat);
forearmR.position.y = -0.115;
forearmR.castShadow = true;
rightElbow.add(forearmR);

const elbowJointR = new THREE.Mesh(new THREE.SphereGeometry(0.046, 24, 24), skinMat);
rightElbow.add(elbowJointR);

const rightWrist = new THREE.Group();
rightWrist.position.set(0, -0.23, 0);
rightElbow.add(rightWrist);

const palmRGeo = new THREE.SphereGeometry(0.045, 24, 24);
palmRGeo.scale(1.2, 0.65, 1.3);
const handR = new THREE.Mesh(palmRGeo, skinMat);
handR.position.set(-0.015, -0.015, 0.025);
handR.castShadow = true;
rightWrist.add(handR);

for (let f = 0; f < 4; f++) {
    const finger = new THREE.Group();
    handR.add(finger);

    const splayX = -0.020 + (f * 0.013); 
    
    const p0 = new THREE.Vector3(-splayX, -0.002, 0.03);
    const p1 = new THREE.Vector3(-splayX * 1.1, 0.006, 0.055);
    const p2 = new THREE.Vector3(-splayX * 1.2, -0.026, 0.075);

    const curve = new THREE.CatmullRomCurve3([p0, p1, p2]);
    const tubeGeo = new THREE.TubeGeometry(curve, 16, fingerRadius, 12, false);
    const tube = new THREE.Mesh(tubeGeo, skinMat);
    tube.castShadow = true;
    finger.add(tube);

    const tipGeo = new THREE.SphereGeometry(fingerRadius, 12, 12);
    const tip = new THREE.Mesh(tipGeo, skinMat);
    tip.position.copy(p2);
    tip.castShadow = true;
    finger.add(tip);

    const baseGeo = new THREE.SphereGeometry(fingerRadius * 1.05, 12, 12);
    const base = new THREE.Mesh(baseGeo, skinMat);
    base.position.copy(p0);
    base.castShadow = true;
    finger.add(base);
}

const thumbR = new THREE.Group();
thumbR.position.set(-0.015, -0.005, 0.015); 
handR.add(thumbR);

const tp0R = new THREE.Vector3(0, 0, 0);
const tp1R = new THREE.Vector3(-0.015, -0.002, 0.015);
const tp2R = new THREE.Vector3(-0.022, -0.012, 0.025);

const thumbCurveR = new THREE.CatmullRomCurve3([tp0R, tp1R, tp2R]);
const thumbMeshR = new THREE.Mesh(new THREE.TubeGeometry(thumbCurveR, 12, thumbRadius, 10, false), skinMat);
thumbMeshR.castShadow = true;
thumbR.add(thumbMeshR);

const thumbTipR = new THREE.Mesh(new THREE.SphereGeometry(thumbRadius, 10, 10), skinMat);
thumbTipR.position.copy(tp2R);
thumbTipR.castShadow = true;
thumbR.add(thumbTipR);

const wristJointR = new THREE.Mesh(new THREE.SphereGeometry(0.036, 24, 24), skinMat);
rightWrist.add(wristJointR);

leftShoulder.rotation.set(-0.80, 0.15, -0.15); 
leftElbow.rotation.set(-0.62, 0, 0);   
leftWrist.rotation.set(0.4, 0.1, 0);

rightShoulder.rotation.set(-0.80, -0.15, 0.15); 
rightElbow.rotation.set(-0.62, 0, 0);  
rightWrist.rotation.set(0.4, -0.1, 0);

const kneeJointGeo = new THREE.SphereGeometry(0.102, 24, 24);
const kneeJointL = new THREE.Mesh(kneeJointGeo, blueMat);
kneeJointL.position.set(-0.11, 0.08, 0.52); 
const kneeJointR = kneeJointL.clone();
kneeJointR.position.x = 0.11;
hips.add(kneeJointL);
hips.add(kneeJointR);

scene.add(charGroup);

const dustCount = 120;
const dustGeo = new THREE.BufferGeometry();
const dustPositions = new Float32Array(dustCount * 3);
for (let i = 0; i < dustCount * 3; i += 3) {
    dustPositions[i] = (Math.random() - 0.5) * 4;     
    dustPositions[i + 1] = Math.random() * 2;         
    dustPositions[i + 2] = (Math.random() - 0.5) * 4; 
}
dustGeo.setAttribute('position', new THREE.BufferAttribute(dustPositions, 3));
const dustMat = new THREE.PointsMaterial({ size: 0.015, color: 0xffdfa6, transparent: true, opacity: 0.6 });
const dustParticles = new THREE.Points(dustGeo, dustMat);
scene.add(dustParticles);

const steamGroup = new THREE.Group();
steamGroup.position.set(-0.55, 0.035, 0.6); 
scene.add(steamGroup);

const steamParticles = [];
const steamCount = 12;
for (let i = 0; i < steamCount; i++) {
    const steamGeo = new THREE.SphereGeometry(0.012, 10, 10);
    const steamMat = new THREE.MeshBasicMaterial({
        color: 0xdddddd,
        transparent: true,
        opacity: 0.0
    });
    const p = new THREE.Mesh(steamGeo, steamMat);
    
    p.position.set(
        (Math.random() - 0.5) * 0.02,
        Math.random() * 0.3,
        (Math.random() - 0.5) * 0.02
    );

    p.userData = {
        speedY: 0.0015 + Math.random() * 0.0015,
        driftSpeed: 0.0006,
        driftFreq: 1 + Math.random() * 2,
        life: Math.random(), 
        decay: 0.004 + Math.random() * 0.004
    };

    steamGroup.add(p);
    steamParticles.push(p);
}

const camStart = new THREE.Vector3(4.5, 8.5, 10.0);   
const camMid = new THREE.Vector3(2.2, 3.2, 4.8);     
const camEnd = new THREE.Vector3(1.6, 1.1, 3.0);     

const targetStart = new THREE.Vector3(0, 0.5, 0);     
const targetEnd = new THREE.Vector3(0, 0.1, 0.3);     

const phase1Duration = 2.5; 
const phase2Duration = 3.5; 
let introFinished = false;

const clock = new THREE.Clock();
let blinkTimer = 0;
let cursorState = true;

function animate() {
    requestAnimationFrame(animate);
    const elapsedTime = clock.getElapsedTime();

    if (!introFinished) {
        if (elapsedTime < phase1Duration) {
            const p1 = elapsedTime / phase1Duration;
            const easeOutCubic = 1 - Math.pow(1 - p1, 3);
            camera.position.lerpVectors(camStart, camMid, easeOutCubic);
            
            const targetProgress = elapsedTime / (phase1Duration + phase2Duration);
            const targetEase = targetProgress < 0.5 ? 4 * targetProgress * targetProgress * targetProgress : 1 - Math.pow(-2 * targetProgress + 2, 3) / 2;
            const currentTarget = targetStart.clone().lerp(targetEnd, targetEase);
            controls.target.copy(currentTarget);
        } 
        else if (elapsedTime < (phase1Duration + phase2Duration)) {
            const p2 = (elapsedTime - phase1Duration) / phase2Duration;
            const easeInOutCubic = p2 < 0.5 ? 4 * p2 * p2 * p2 : 1 - Math.pow(-2 * p2 + 2, 3) / 2;
            camera.position.lerpVectors(camMid, camEnd, easeInOutCubic);

            const targetProgress = elapsedTime / (phase1Duration + phase2Duration);
            const targetEase = targetProgress < 0.5 ? 4 * targetProgress * targetProgress * targetProgress : 1 - Math.pow(-2 * targetProgress + 2, 3) / 2;
            const currentTarget = targetStart.clone().lerp(targetEnd, targetEase);
            controls.target.copy(currentTarget);
        } 
        else {
            camera.position.copy(camEnd);
            controls.target.copy(targetEnd);
            controls.enabled = true;
            introFinished = true;
        }
    }

    blinkTimer += 0.016;
    if (blinkTimer >= 0.5) { 
        cursorState = !cursorState;
        blinkTimer = 0;
        drawCodeScreen(cursorState);
    }

    const breathingCycle = Math.sin(elapsedTime * 1.5);
    torso.rotation.x = 0.08 + (breathingCycle * 0.012); 
    headGroup.rotation.x = (breathingCycle * 0.015);    
    headGroup.position.y = 0.73 + (breathingCycle * 0.005); 
    
    leftWrist.rotation.x = 0.4 + Math.sin(elapsedTime * 12) * 0.04;
    rightWrist.rotation.x = 0.4 + Math.cos(elapsedTime * 15) * 0.04;

    const positions = dustParticles.geometry.attributes.position.array;
    for (let i = 0; i < dustCount * 3; i += 3) {
        positions[i + 1] -= 0.001; 
        positions[i] += Math.sin(elapsedTime + i) * 0.0005; 
        if (positions[i + 1] < 0) {
            positions[i + 1] = 2; 
        }
    }
    dustParticles.geometry.attributes.position.needsUpdate = true;

    steamParticles.forEach(p => {
        p.userData.life += p.userData.decay;
        if (p.userData.life > 1.0) {
            p.userData.life = 0;
            p.position.set(
                (Math.random() - 0.5) * 0.015,
                0,
                (Math.random() - 0.5) * 0.015
            );
        }
        
        p.position.y += p.userData.speedY;
        
        p.position.x += Math.sin(elapsedTime * p.userData.driftFreq + p.position.y * 4) * p.userData.driftSpeed;
        p.position.z += Math.cos(elapsedTime * p.userData.driftFreq + p.position.y * 4) * p.userData.driftSpeed;

        let opacity = 0;
        if (p.userData.life < 0.25) {
            opacity = (p.userData.life / 0.25) * 0.12;
        } else {
            opacity = (1.0 - (p.userData.life - 0.25) / 0.75) * 0.12;
        }
        p.material.opacity = Math.max(0, opacity);

        const scale = 1.0 + p.userData.life * 2.2;
        p.scale.set(scale, scale, scale);
    });

    controls.update();
    renderer.render(scene, camera);
}

function onWindowResize() {
    const c = document.getElementById('canvas-container');
    const width = c ? c.clientWidth : window.innerWidth;
    const height = c ? c.clientHeight : window.innerHeight;
    
    camera.aspect = width / height;

    if (camera.aspect < 1) {
        camera.fov = Math.min(75, 45 / (camera.aspect * 0.95));
    } else {
        camera.fov = 45;
    }

    camera.updateProjectionMatrix();
    renderer.setSize(width, height);
}

onWindowResize();
window.addEventListener('resize', onWindowResize, false);

drawCodeScreen(true);
animate();