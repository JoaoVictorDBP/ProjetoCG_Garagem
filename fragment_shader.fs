#version 330 core
out vec4 FragColor;

in vec2 out_texture;
in vec3 FragPos;
in vec3 Normal;

uniform sampler2D imagem;

// Identifica se o objeto pertence ao ambiente externo (0)
// ou ao ambiente interno (1). Essa informação é utilizada
// no fragment shader para que as luzes de um ambiente não
// afetem os objetos do outro ambiente.

uniform int ambienteID; 

// Fontes de Luz Externas (Carro)

// Calcula a posição dos faróis em relação à posição e
// orientação atual do carro, permitindo que a fonte
// luminosa acompanhe a movimentação do veículo.

uniform vec3 farolEsqPos;
uniform vec3 farolDirPos;
uniform vec3 farolColor;

uniform vec3 farolEsqDir;
uniform vec3 farolDirDir;

uniform float cutOff;
uniform float outerCutOff;

// Fontes de Luz Internas (Garagem)

// Fonte luminosa do ambiente interno (lâmpada do teto)
uniform vec3 lampadaTetoPos;
uniform vec3 lampadaTetoColor; 
uniform vec3 segundaLuzPos;    
uniform vec3 segundaLuzColor;  

uniform vec3 lampadaTetoDir;
uniform vec3 segundaLuzDir;

// Segunda fonte luminosa do ambiente interno (lanterna)
uniform float cutOffInternoLampada;
uniform float outerCutOffInternoLampada;

uniform float cutOffInternoLanterna;
uniform float outerCutOffInternoLanterna;

uniform vec3 viewPos;

uniform float globalAmbientStrength;
uniform float globalDiffuseStrength;
uniform float globalSpecularStrength;


// Fonte luminosa do ambiente externo (poste)
uniform bool postesOn;

// Parâmetros de material definidos manualmente para cada
// objeto da cena. Não são utilizados valores provenientes
// de arquivos .mtl, conforme exigido pelo projeto.

uniform float matAmbient;
uniform float matDiffuse;
uniform float matSpecular;
uniform float matShininess; 

float calculaSpotlight(vec3 lightPos, vec3 lightDirSpot, vec3 fragPos)
{
    vec3 dirToFrag = normalize(fragPos - lightPos);
    float theta = dot(dirToFrag, normalize(lightDirSpot));
    float epsilon = cutOff - outerCutOff;
    return clamp((theta - outerCutOff) / epsilon, 0.0, 1.0);
}

float calculaSpotlightInternoLampada(
    vec3 lightPos,
    vec3 lightDirSpot,
    vec3 fragPos)
{
    vec3 dirToFrag = normalize(fragPos - lightPos);

    float theta =
        dot(dirToFrag, normalize(lightDirSpot));

    float epsilon =
        cutOffInternoLampada - outerCutOffInternoLampada;

    return clamp(
        (theta - outerCutOffInternoLampada) / epsilon,
        0.0,
        1.0
    );
}

float calculaSpotlightInternoLanterna(
    vec3 lightPos,
    vec3 lightDirSpot,
    vec3 fragPos)
{
    vec3 dirToFrag = normalize(fragPos - lightPos);

    float theta =
        dot(dirToFrag, normalize(lightDirSpot));

    float epsilon =
        cutOffInternoLanterna - outerCutOffInternoLanterna;

    return clamp(
        (theta - outerCutOffInternoLanterna) / epsilon,
        0.0,
        1.0
    );
}

void main()
{
    vec3 textureColor = texture(imagem, out_texture).rgb;
    vec3 norm = normalize(Normal);
    vec3 viewDir = normalize(viewPos - FragPos);

    // Se o matAmbient for muito alto (indicando que é uma lâmpada/farol ligado),
    // fazemos o objeto ignorar a escuridão e brilhar por completo.
    if (matAmbient >= 0.9) {
        vec3 corEmissivaEsfera = (textureColor == vec3(0.0)) ? vec3(1.0, 0.9, 0.6) : textureColor;
        FragColor = vec4(corEmissivaEsfera * 2.5, 1.0); 
        return;
    }

    vec3 totalAmbient = vec3(0.0);
    vec3 totalDiffuse = vec3(0.0);
    vec3 totalSpecular = vec3(0.0);

    // ==========================================
    // SE FOR OBJETO DO AMBIENTE EXTERNO (0)
    // ==========================================
    if (ambienteID == 0) {
        totalAmbient = globalAmbientStrength * matAmbient * vec3(1.0, 1.0, 1.0);

        // --- FAROL ESQUERDO ---
        float distEsq = length(farolEsqPos - FragPos);
        float attEsq = 1.0 / (1.0 + 0.035 * distEsq + 0.005 * (distEsq * distEsq));

        vec3 lightDirEsq = normalize(farolEsqPos - FragPos);
        float diffEsq = max(dot(norm, lightDirEsq), 0.0);
        float spotEsq = calculaSpotlight(farolEsqPos, farolEsqDir, FragPos);
        
        vec3 diffuseEsq = globalDiffuseStrength * matDiffuse * diffEsq * farolColor * attEsq * spotEsq;
        vec3 reflectDirEsq = reflect(-lightDirEsq, norm);
        float specEsq = pow(max(dot(viewDir, reflectDirEsq), 0.0), matShininess);
        vec3 specularEsq = globalSpecularStrength * matSpecular * specEsq * farolColor * attEsq * spotEsq;

        // --- FAROL DIREITO ---
        float distDir = length(farolDirPos - FragPos);
        float attDir = 1.0 / (1.0 + 0.035 * distDir + 0.005 * (distDir * distDir));

        vec3 lightDirDir = normalize(farolDirPos - FragPos);
        float diffDir = max(dot(norm, lightDirDir), 0.0);
        float spotDir = calculaSpotlight(farolDirPos, farolDirDir, FragPos);
        
        vec3 diffuseDir = globalDiffuseStrength * matDiffuse * diffDir * farolColor * attDir * spotDir;
        vec3 reflectDirDir = reflect(-lightDirDir, norm);
        float specDir = pow(max(dot(viewDir, reflectDirDir), 0.0), matShininess);
        vec3 specularDir = globalSpecularStrength * matSpecular * specDir * farolColor * attDir * spotDir;

        // --- ILUMINAÇÃO DOS POSTES ---
        vec3 luzPostesDiffuse = vec3(0.0);
        vec3 luzPostesSpecular = vec3(0.0);

        if(postesOn)
        {
            vec3 corLuzPoste = vec3(2.0, 1.6, 0.9);

            float posPostesX[6] = float[](5.0, 35.0, 65.0, -25.0, -55.0, -85.0);
            float posPostesZ[1] = float[](12.5);
            float alturaLuzPoste = 4.5;

            for(int i = 0; i < 6; i++)
            {
                for(int j = 0; j < 1; j++)
                {
                    vec3 posLuz = vec3(posPostesX[i], alturaLuzPoste, posPostesZ[j]);
                    float dPoste = length(posLuz - FragPos);

                    vec3 dirToFragPoste = normalize(FragPos - posLuz);
                    vec3 dirConePoste = vec3(0.0, -1.0, 0.0);

                    float spotEfeito = dot(dirToFragPoste, dirConePoste);

                    if(spotEfeito > 0.85)
                    {
                        float intensidadeSpot =
                            clamp((spotEfeito - 0.85) / (1.0 - 0.85), 0.0, 1.0);

                        float attPoste =
                            1.0 / (1.0 + 0.13 * dPoste + 0.045 * (dPoste * dPoste));

                        vec3 lightDirPoste = normalize(posLuz - FragPos);

                        float diffPoste =
                            max(dot(norm, lightDirPoste), 0.0);

                        luzPostesDiffuse +=
                            1.15 *
                            globalDiffuseStrength *
                            matDiffuse *
                            diffPoste *
                            corLuzPoste *
                            attPoste *
                            intensidadeSpot;

                        vec3 reflectDirPoste =
                            reflect(-lightDirPoste, norm);

                        float specPoste =
                            pow(max(dot(viewDir, reflectDirPoste), 0.0),
                                matShininess);

                        luzPostesSpecular +=
                            globalSpecularStrength *
                            matSpecular *
                            specPoste *
                            corLuzPoste *
                            attPoste *
                            intensidadeSpot;
                    }
                }
            }
        }

        totalDiffuse = diffuseEsq + diffuseDir + luzPostesDiffuse;
        totalSpecular = specularEsq + specularDir + luzPostesSpecular;
    }
    
    // ==========================================
    // SE FOR OBJETO DO AMBIENTE INTERNO (1)
    // ==========================================
    else if (ambienteID == 1) {
        totalAmbient = globalAmbientStrength * matAmbient * vec3(1.0, 1.0, 1.0);

        // --- LÂMPADA DO TETO ---
        float distTeto =length(lampadaTetoPos - FragPos);
        float attTeto =1.0 /(1.0 + 0.05 * distTeto +0.01 * (distTeto * distTeto));

        vec3 lightDirTeto =normalize(lampadaTetoPos - FragPos);
        float diffTeto =max(dot(norm, lightDirTeto), 0.0);
        float spotTeto =calculaSpotlightInternoLampada(lampadaTetoPos,lampadaTetoDir,FragPos);

        vec3 diffuseTeto =globalDiffuseStrength *matDiffuse *diffTeto *lampadaTetoColor *attTeto *spotTeto;
        vec3 reflectDirTeto =reflect(-lightDirTeto, norm);

        float specTeto =pow(max(dot(viewDir, reflectDirTeto), 0.0),matShininess);
        vec3 specularTeto =globalSpecularStrength *matSpecular *specTeto *lampadaTetoColor *attTeto *spotTeto;

        // --- SEGUNDA LUZ INTERNA ---
        float distSeg =length(segundaLuzPos - FragPos);
        float attSeg =1.0 /(1.0 + 0.08 * distSeg +0.03 * (distSeg * distSeg));

        vec3 lightDirSeg =normalize(segundaLuzPos - FragPos);
        float diffSeg =max(dot(norm, lightDirSeg), 0.0);
        float spotSeg =calculaSpotlightInternoLanterna(segundaLuzPos,segundaLuzDir,FragPos);

        vec3 diffuseSeg =globalDiffuseStrength *matDiffuse *diffSeg *segundaLuzColor *attSeg *spotSeg;
        vec3 reflectDirSeg =reflect(-lightDirSeg, norm);

        float specSeg =pow(max(dot(viewDir, reflectDirSeg), 0.0),matShininess);
        vec3 specularSeg =globalSpecularStrength *matSpecular *specSeg *segundaLuzColor *attSeg *spotSeg;

        totalDiffuse =diffuseTeto +diffuseSeg;
        totalSpecular =specularTeto +specularSeg;
    }

    vec3 finalLight = totalAmbient + totalDiffuse + totalSpecular;
    
    // Filtro para a Skybox
    if(matAmbient >= 0.99 && matDiffuse == 0.0 && matSpecular == 0.0) {
        finalLight = vec3(1.0);
    }

    FragColor = vec4(finalLight * textureColor, 1.0);
}