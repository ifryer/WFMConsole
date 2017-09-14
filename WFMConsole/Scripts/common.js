function toUrl(dest) {
    var basePath = location.pathname.substr(1).match(/[a-z_0-1]*/i);
    basePath = "/" + basePath + "/";

    return basePath + dest;
}