from OpenGL.GL import *

class Shader:
    def __init__(self, vertexPath: str, fragmentPath: str):
        try:
            vShaderFile = open(vertexPath, "r", encoding="utf-8")
            fShaderFile = open(fragmentPath, "r", encoding="utf-8")
            
            vertexCode = vShaderFile.read()
            fragmentCode = fShaderFile.read()
            
            vShaderFile.close()
            fShaderFile.close()

            vertex = glCreateShader(GL_VERTEX_SHADER)
            glShaderSource(vertex, vertexCode)
            glCompileShader(vertex)
            self.checkCompileErrors(vertex, "VERTEX")
            
            fragment = glCreateShader(GL_FRAGMENT_SHADER)
            glShaderSource(fragment, fragmentCode)
            glCompileShader(fragment)
            self.checkCompileErrors(fragment, "FRAGMENT")
            
            self.ID = glCreateProgram()
            glAttachShader(self.ID, vertex)
            glAttachShader(self.ID, fragment)
            glLinkProgram(self.ID)
            self.checkCompileErrors(self.ID, "PROGRAM")
            
            glDeleteShader(vertex)
            glDeleteShader(fragment)
        
        except IOError:
            print("ERROR::SHADER::FILE_NOT_SUCCESFULLY_READ")

    def use(self) -> None:
        glUseProgram(self.ID)

    def getProgram(self) -> int:
        return self.ID

    def setBool(self, name: str, value: bool) -> None:
        glUniform1i(glGetUniformLocation(self.ID, name), int(value))

    def setInt(self, name: str, value: int) -> None:
        glUniform1i(glGetUniformLocation(self.ID, name), value)

    def setFloat(self, name: str, value: float) -> None:
        glUniform1f(glGetUniformLocation(self.ID, name), value)

    def setVec3(self, name: str, x: float, y: float, z: float) -> None:
        glUniform3f(glGetUniformLocation(self.ID, name), x, y, z)

    def setVec4(self, name: str, x: float, y: float, z: float, w: float) -> None:
        glUniform4f(glGetUniformLocation(self.ID, name), x, y, z, w)

    def checkCompileErrors(self, shader: int, type: str) -> None:
        if type != "PROGRAM":
            success = glGetShaderiv(shader, GL_COMPILE_STATUS)
            if not success:
                infoLog = glGetShaderInfoLog(shader)
                print("ERROR::SHADER_COMPILATION_ERROR of type: " + type + "\n" + infoLog.decode() + "\n")
        else:
            success = glGetProgramiv(shader, GL_LINK_STATUS)
            if not success:
                infoLog = glGetProgramInfoLog(shader)
                print("ERROR::PROGRAM_LINKING_ERROR of type: " + type + "\n" + infoLog.decode() + "\n")