import torch
import torch.nn as nn
import torch.nn.functional as F
import onnx
import onnxruntime
from pathlib import Path
import sys
import numpy as np

CHECKPOINT = Path(r"C:\source\repos\Resnet-101-AP-GeM.pt")
OUT_ONNX = Path("resnet101_ap_gem.onnx")

class MockPCA:
    def __init__(self, n_components=None):
        self.n_components = n_components
    def fit(self, X):
        return self
    def transform(self, X):
        return X

sklearn_decomposition = type(sys)('sklearn.decomposition')
sklearn_decomposition.pca = type(sys)('sklearn.decomposition.pca')
sklearn_decomposition.pca.PCA = MockPCA
sys.modules['sklearn.decomposition'] = sklearn_decomposition
sys.modules['sklearn.decomposition.pca'] = sklearn_decomposition.pca

state = torch.load(CHECKPOINT, map_location="cpu", weights_only=False)
state_dict = state['state_dict']

class GeMPooling(nn.Module):
    def __init__(self, p=3.0, eps=1e-6):
        super().__init__()
        self.p = nn.Parameter(torch.tensor([p], dtype=torch.float32))
        self.eps = eps

    def forward(self, x):
        return F.avg_pool2d(x.clamp(min=self.eps).pow(self.p), (x.size(2), x.size(3))).pow(1.0 / self.p)

class Bottleneck(nn.Module):
    def __init__(self, in_channels, out_channels, stride=1, downsample=None):
        super().__init__()
        self.conv1 = nn.Conv2d(in_channels, out_channels//4, kernel_size=1, bias=False)
        self.bn1 = nn.BatchNorm2d(out_channels//4)
        self.conv2 = nn.Conv2d(out_channels//4, out_channels//4, kernel_size=3, stride=stride, padding=1, bias=False)
        self.bn2 = nn.BatchNorm2d(out_channels//4)
        self.conv3 = nn.Conv2d(out_channels//4, out_channels, kernel_size=1, bias=False)
        self.bn3 = nn.BatchNorm2d(out_channels)
        self.relu = nn.ReLU(inplace=True)
        self.downsample = downsample
        self.stride = stride

    def forward(self, x):
        identity = x
        out = self.conv1(x)
        out = self.bn1(out)
        out = self.relu(out)
        out = self.conv2(out)
        out = self.bn2(out)
        out = self.relu(out)
        out = self.conv3(out)
        out = self.bn3(out)
        if self.downsample is not None:
            identity = self.downsample(x)
        out += identity
        out = self.relu(out)
        return out

class ResNet101APGeM(nn.Module):
    def __init__(self, output_dim=2048):
        super().__init__()
        self.conv1 = nn.Conv2d(3, 64, kernel_size=7, stride=2, padding=3, bias=False)
        self.bn1 = nn.BatchNorm2d(64)
        self.relu = nn.ReLU(inplace=True)
        self.maxpool = nn.MaxPool2d(kernel_size=3, stride=2, padding=1)
        self.layer1 = self._make_layer(64, 256, 3)
        self.layer2 = self._make_layer(256, 512, 4, stride=2)
        self.layer3 = self._make_layer(512, 1024, 23, stride=2)
        self.layer4 = self._make_layer(1024, 2048, 3, stride=2)
        self.gem = GeMPooling()
        self.fc = nn.Linear(2048, output_dim)

    def _make_layer(self, in_channels, out_channels, blocks, stride=1):
        downsample = None
        if stride != 1 or in_channels != out_channels:
            downsample = nn.Sequential(
                nn.Conv2d(in_channels, out_channels, kernel_size=1, stride=stride, bias=False),
                nn.BatchNorm2d(out_channels),
            )
        layers = []
        layers.append(Bottleneck(in_channels, out_channels, stride, downsample))
        for _ in range(1, blocks):
            layers.append(Bottleneck(out_channels, out_channels))
        return nn.Sequential(*layers)

    def forward(self, x):
        mean = torch.tensor([0.485, 0.456, 0.406], device=x.device).view(1, 3, 1, 1)
        std  = torch.tensor([0.229, 0.224, 0.225], device=x.device).view(1, 3, 1, 1)
        x = (x - mean) / std
        x = self.conv1(x)
        x = self.bn1(x)
        x = self.relu(x)
        x = self.maxpool(x)
        x = self.layer1(x)
        x = self.layer2(x)
        x = self.layer3(x)
        x = self.layer4(x)
        x = self.gem(x)
        x = torch.flatten(x, 1)
        x = self.fc(x)
        x = F.normalize(x, p=2, dim=1)
        return x

model = ResNet101APGeM()

new_state_dict = {}
for k, v in state_dict.items():
    new_key = k.replace('module.', '')
    new_state_dict[new_key] = v

if 'adpool.p' in new_state_dict:
    new_state_dict['gem.p'] = new_state_dict.pop('adpool.p')

missing, unexpected = model.load_state_dict(new_state_dict, strict=False)

print(f"Missing: {len(missing)}")
print(f"Unexpected: {len(unexpected)}")

if len(missing) == 0 and len(unexpected) == 0:
    print("All parameters loaded successfully")
else:
    if missing:
        print(f"Missing keys: {list(missing)}")
    if unexpected:
        print(f"Unexpected keys: {list(unexpected)}")

model.eval()

with torch.no_grad():
    img = torch.randn(1, 3, 224, 224)
    emb = model(img)
    norm = emb.norm(dim=1).item()
    print("Norm:", norm)

    sim_self = torch.matmul(emb, emb.T).item()
    print("Self similarity:", sim_self)

    noisy = img + torch.randn_like(img) * 0.01
    emb_noisy = model(noisy)
    sim_noise = torch.matmul(emb, emb_noisy.T).item()
    print("Perturb similarity:", sim_noise)

torch.onnx.export(
    model,
    img,
    OUT_ONNX,
    opset_version=18,
    input_names=["input"],
    output_names=["embedding"],
    dynamic_axes={"input": {0: "batch"}, "embedding": {0: "batch"}},
    do_constant_folding=True
)

onnx_model = onnx.load(OUT_ONNX)
onnx.checker.check_model(onnx_model)

sess = onnxruntime.InferenceSession(str(OUT_ONNX), providers=["CPUExecutionProvider"])
onnx_out = sess.run(None, {"input": img.numpy()})[0]

diff = np.abs(emb.numpy() - onnx_out).max()
print(f"Max diff: {diff}")
print(f"ONNX opset: {onnx_model.opset_import[0].version}")
