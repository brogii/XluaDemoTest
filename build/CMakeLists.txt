# Tencent is pleased to support the open source community by making xLua available.
# Copyright (C) 2016 THL A29 Limited, a Tencent company. All rights reserved.
# Licensed under the MIT License (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
# http://opensource.org/licenses/MIT
# Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

cmake_minimum_required(VERSION 2.8)

if ( WIN32 AND NOT CYGWIN )
	set(CMAKE_C_FLAGS_RELEASE "${CMAKE_C_FLAGS_RELEASE} /MT" CACHE STRING "")
	set(CMAKE_C_FLAGS_DEBUG "${CMAKE_C_FLAGS_DEBUG} /MTd" CACHE STRING "")
	set(CMAKE_CXX_FLAGS_RELEASE "${CMAKE_CXX_FLAGS_RELEASE} /MT" CACHE STRING "")
	set(CMAKE_CXX_FLAGS_DEBUG "${CMAKE_CXX_FLAGS_DEBUG} /MTd" CACHE STRING "")
endif ()

project(XLua)

option ( UINT_ESPECIALLY "using custom ulong" OFF )
option ( USING_LUAJIT "using luajit" OFF )

find_path(XLUA_PROJECT_DIR NAMES SConstruct
    PATHS 
    ${CMAKE_SOURCE_DIR}
    NO_DEFAULT_PATH
    )

MARK_AS_ADVANCED(XLUA_PROJECT_DIR)

#begin lua-rapidjson
set (RAPIDJSON_SRC lua-rapidjson/source/rapidjson.cpp)
set_property(
	SOURCE ${RAPIDJSON_SRC}
	APPEND
	PROPERTY COMPILE_DEFINITIONS
	LUA_LIB
)
list(APPEND THIRDPART_INC  lua-rapidjson/include)
set (THIRDPART_SRC ${THIRDPART_SRC} ${RAPIDJSON_SRC})
#end lua-rapidjson



if (USING_LUAJIT)
	
    if (APPLE AND NOT IOS)
	    include_directories(
		  ${CMAKE_SOURCE_DIR}
		  lua-5.1.5/src
		  lua-5.1.5/etc
		  ${CMAKE_CURRENT_BINARY_DIR}
		  ${THIRDPART_INC}
	    )
	    set ( LUA_CORE lua-5.1.5/src/lapi.c lua-5.1.5/src/lcode.c lua-5.1.5/src/ldebug.c lua-5.1.5/src/ldo.c lua-5.1.5/src/ldump.c lua-5.1.5/src/lfunc.c 
          lua-5.1.5/src/lgc.c lua-5.1.5/src/llex.c lua-5.1.5/src/lmem.c lua-5.1.5/src/lobject.c lua-5.1.5/src/lopcodes.c lua-5.1.5/src/lparser.c lua-5.1.5/src/lstate.c 
          lua-5.1.5/src/lstring.c lua-5.1.5/src/ltable.c lua-5.1.5/src/ltm.c lua-5.1.5/src/lundump.c lua-5.1.5/src/lvm.c lua-5.1.5/src/lzio.c lua-5.1.5/src/lauxlib.c 
          lua-5.1.5/src/lbaselib.c lua-5.1.5/src/ldblib.c lua-5.1.5/src/liolib.c lua-5.1.5/src/lmathlib.c lua-5.1.5/src/loslib.c lua-5.1.5/src/ltablib.c 
          lua-5.1.5/src/lstrlib.c lua-5.1.5/src/linit.c lua-5.1.5/src/loadlib.c)
	else ()
	    include_directories(
		  ${CMAKE_SOURCE_DIR}
		  luajit-2.1.0b2/src
		  ${CMAKE_CURRENT_BINARY_DIR}
		  ${THIRDPART_INC}
	    )

	    set ( LUA_CORE )
    endif ()
	set ( LUA_LIB )
else ()
	set ( LUA_IDSIZE 120 CACHE NUMBER "gives the maximum size for the description of the source." )

	configure_file ( lua-5.3.3/src/luaconf.h.in ${CMAKE_CURRENT_BINARY_DIR}/luaconf.h )

	include_directories(
	    ${CMAKE_SOURCE_DIR}
		lua-5.3.3/src
		${CMAKE_CURRENT_BINARY_DIR}
		${THIRDPART_INC}
	)

	set ( LUA_CORE lua-5.3.3/src/lapi.c lua-5.3.3/src/lcode.c lua-5.3.3/src/lctype.c lua-5.3.3/src/ldebug.c lua-5.3.3/src/ldo.c lua-5.3.3/src/ldump.c
	  lua-5.3.3/src/lfunc.c lua-5.3.3/src/lgc.c lua-5.3.3/src/llex.c lua-5.3.3/src/lmem.c lua-5.3.3/src/lobject.c lua-5.3.3/src/lopcodes.c lua-5.3.3/src/lparser.c
	  lua-5.3.3/src/lstate.c lua-5.3.3/src/lstring.c lua-5.3.3/src/ltable.c lua-5.3.3/src/ltm.c lua-5.3.3/src/lundump.c lua-5.3.3/src/lvm.c lua-5.3.3/src/lzio.c )
	set ( LUA_LIB lua-5.3.3/src/lauxlib.c lua-5.3.3/src/lbaselib.c lua-5.3.3/src/lbitlib.c lua-5.3.3/src/lcorolib.c lua-5.3.3/src/ldblib.c
	  lua-5.3.3/src/liolib.c lua-5.3.3/src/lmathlib.c lua-5.3.3/src/loslib.c lua-5.3.3/src/lstrlib.c lua-5.3.3/src/ltablib.c lua-5.3.3/src/linit.c
	  lua-5.3.3/src/lutf8lib.c lua-5.3.3/src/loadlib.c )
endif ()

set ( LUA_SOCKET
    luasocket/auxiliar.c
    luasocket/buffer.c
    luasocket/except.c
    luasocket/inet.c
    luasocket/io.c
    luasocket/luasocket.c
    luasocket/mime.c
    luasocket/options.c
    luasocket/select.c
    luasocket/tcp.c
    luasocket/timeout.c
    luasocket/udp.c 
)

if ( WIN32 AND NOT CYGWIN )
    list (APPEND LUA_SOCKET luasocket/wsocket.c)
else ( )
    list (APPEND LUA_SOCKET luasocket/usocket.c)
endif ( )

set ( XLUA_CORE
    i64lib.c
    perflib.c
    xlua.c
)

if (APPLE)
    if (IOS)
        set(CMAKE_OSX_ARCHITECTURES "$(ARCHS_STANDARD)")
        add_library(xlua STATIC
           ${LUA_CORE}
           ${LUA_LIB}
           ${LUA_SOCKET}
           ${XLUA_CORE}
           ${THIRDPART_SRC}
        )
    else ()
        set(CMAKE_OSX_ARCHITECTURES "$(ARCHS_STANDARD_32_64_BIT)")
        add_library(xlua MODULE
            ${LUA_CORE}
            ${LUA_LIB}
            ${LUA_SOCKET}
            ${XLUA_CORE}
            ${THIRDPART_SRC}
        )
        set_target_properties ( xlua PROPERTIES BUNDLE TRUE )
        #set_target_properties ( xlua PROPERTIES FRAMEWORK TRUE )
        #set_target_properties ( xlua PROPERTIES MACOSX_RPATH TRUE )
    endif ()
else ( )
    add_library(xlua SHARED
        ${LUA_CORE}
        ${LUA_LIB}
        ${LUA_SOCKET}
        ${XLUA_CORE}
        ${THIRDPART_SRC}
    )
endif ( )

if ( WIN32 AND NOT CYGWIN )
    target_compile_definitions (xlua PRIVATE LUA_BUILD_AS_DLL)
endif ( )

set_property(
	SOURCE ${LUA_SOCKET}
	APPEND
	PROPERTY COMPILE_DEFINITIONS
	LUA_COMPAT_APIINTCASTS LUA_LIB
)

if(UINT_ESPECIALLY)
    ADD_DEFINITIONS(-DUINT_ESPECIALLY)
endif()
	
if ( WIN32 AND NOT CYGWIN )
    if (USING_LUAJIT)
        target_link_libraries(xlua
            ${CMAKE_SOURCE_DIR}/luajit-2.1.0b2/src/lua51.lib
            ws2_32
            ${THIRDPART_LIB} 
        )
    else ()
        target_link_libraries(xlua
            ws2_32
            ${THIRDPART_LIB} 
        )
    endif()
else ( )
    if (USING_LUAJIT AND NOT APPLE)
		target_link_libraries(xlua
			${CMAKE_SOURCE_DIR}/luajit-2.1.0b2/src/libluajit.a
			m
            ${THIRDPART_LIB} 
		)
    else ()
        target_link_libraries(xlua
            m
            ${THIRDPART_LIB} 
        )
    endif()
endif ( )
